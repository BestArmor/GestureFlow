using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
// 🔑 Алиас для разрешения конфликта WPF/WinForms
using Application = System.Windows.Application;

namespace GestureFlow.Core
{
    public class MouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MOUSEMOVE = 0x0200;

        private IntPtr _hookId = IntPtr.Zero;
        private HookProc? _proc;

        private bool _tracking;
        private bool _inGesture;
        private int _startX, _startY;
        private long _startTime;
        private volatile bool _isSimulating;

        private readonly ConcurrentQueue<MouseEvent> _queue = new();
        private readonly CancellationTokenSource _cts = new();
        private Task? _consumerTask;

        private readonly double _moveThresholdPx;
        private readonly int _holdThresholdMs;

        public event Action<int, int>? MouseMovedWhilePressed;
        public event Action<int, int>? MouseButtonReleased;
        public event Action<int, int>? MousePressed;

        public MouseHook(double moveThresholdPx = 5.0, int holdThresholdMs = 100)
        {
            _moveThresholdPx = moveThresholdPx;
            _holdThresholdMs = holdThresholdMs;
            _proc = HookCallback;
        }

        public void Start()
        {
            using var proc = Process.GetCurrentProcess();
            using var mod = proc.MainModule;
            if (mod != null)
            {
                _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc!,
                    GetModuleHandle(mod.ModuleName), 0);
            }
            _consumerTask = Task.Run(() => ProcessQueue(_cts.Token));
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_isSimulating)
                return CallNextHookEx(_hookId, nCode, wParam, lParam);

            if (nCode >= 0)
            {
                var hook = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                int msg = wParam.ToInt32();
                int x = hook.pt.X, y = hook.pt.Y;

                if (msg == WM_RBUTTONDOWN)
                {
                    _tracking = true;
                    _inGesture = false;
                    _startX = x;
                    _startY = y;
                    _startTime = Environment.TickCount64;
                    _queue.Enqueue(new MouseEvent(MouseEventType.Down, x, y));
                    return CallNextHookEx(_hookId, nCode, wParam, lParam);
                }
                else if (msg == WM_RBUTTONUP)
                {
                    if (_tracking)
                    {
                        if (_inGesture)
                        {
                            _queue.Enqueue(new MouseEvent(MouseEventType.UpGesture, x, y));
                            _tracking = false;
                            _inGesture = false;
                            return (IntPtr)1;
                        }
                        else
                        {
                            _queue.Enqueue(new MouseEvent(MouseEventType.UpNormal, x, y));
                            _tracking = false;
                            return CallNextHookEx(_hookId, nCode, wParam, lParam);
                        }
                    }
                    return CallNextHookEx(_hookId, nCode, wParam, lParam);
                }
                else if (msg == WM_MOUSEMOVE && _tracking)
                {
                    if (!_inGesture)
                    {
                        double dx = x - _startX;
                        double dy = y - _startY;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        long elapsed = Environment.TickCount64 - _startTime;

                        if (dist > _moveThresholdPx || elapsed > _holdThresholdMs)
                        {
                            _inGesture = true;
                            _queue.Enqueue(new MouseEvent(MouseEventType.GestureStarted, _startX, _startY));
                        }
                    }

                    if (_inGesture)
                    {
                        _queue.Enqueue(new MouseEvent(MouseEventType.Move, x, y));
                    }
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void ProcessQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var ev))
                {
                    try
                    {
                        // 🔑 КРИТИЧНО: маршалим events в UI-поток для WPF
                        var dispatcher = System.Windows.Application.Current?.Dispatcher;
                        
                        switch (ev.Type)
                        {
                            case MouseEventType.Down:
                                dispatcher?.BeginInvoke(() => MousePressed?.Invoke(ev.X, ev.Y));
                                break;

                            case MouseEventType.UpNormal:
                                break;

                            case MouseEventType.UpGesture:
                                Task.Run(() => SimulateRightButtonUp());
                                dispatcher?.BeginInvoke(() => MouseButtonReleased?.Invoke(ev.X, ev.Y));
                                break;

                            case MouseEventType.Move:
                                dispatcher?.BeginInvoke(() => MouseMovedWhilePressed?.Invoke(ev.X, ev.Y));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ ProcessQueue error: {ex.Message}");
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void SimulateRightButtonUp()
        {
            const uint INPUT_MOUSE = 0;
            const uint MOUSEEVENTF_RIGHTUP = 0x0010;

            _isSimulating = true;
            try
            {
                var input = new INPUT
                {
                    type = INPUT_MOUSE,
                    U = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = 0, dy = 0, mouseData = 0,
                            dwFlags = MOUSEEVENTF_RIGHTUP,
                            time = 0, dwExtraInfo = IntPtr.Zero
                        }
                    }
                };
                SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
            }
            finally
            {
                _isSimulating = false;
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _consumerTask?.Wait(TimeSpan.FromSeconds(1));
            _cts.Dispose();
            if (_hookId != IntPtr.Zero)
                UnhookWindowsHookEx(_hookId);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, HookProc fn, IntPtr mod, uint tid);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wp, IntPtr lp);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private delegate IntPtr HookProc(int code, IntPtr wp, IntPtr lp);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData, flags, time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT { public uint type; public InputUnion U; }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion { [FieldOffset(0)] public MOUSEINPUT mi; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx, dy;
            public uint mouseData, dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        private enum MouseEventType { Down, UpNormal, UpGesture, GestureStarted, Move }

        private readonly struct MouseEvent
        {
            public MouseEventType Type { get; }
            public int X { get; }
            public int Y { get; }
            public MouseEvent(MouseEventType type, int x, int y) { Type = type; X = x; Y = y; }
        }
    }
}