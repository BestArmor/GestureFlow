using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

// 🔑 Алиас для разрешения конфликта WPF/WinForms
using Application = System.Windows.Application;

namespace GestureFlow.Core
{
    /// <summary>
    /// 🖱️ Глобальный хук мыши через Win32 API.
    /// Отслеживает нажатие и движение правой кнопки мыши.
    /// </summary>
    public class MouseHook : IDisposable
    {
        // 🔑 БЕЗ readonly — чтобы можно было обновлять через UpdateThresholds
        private double _moveThresholdPx;
        private int _holdThresholdMs;

        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelMouseProc? _proc;
        private bool _isRightButtonPressed = false;
        private DateTime _pressStartTime;
        private System.Windows.Point _pressStartPoint;
        private System.Windows.Point _lastPoint;
        private bool _isDragging = false;

        /// <summary>
        /// Срабатывает когда мышь движется с зажатой ПКМ
        /// </summary>
        public event Action<double, double>? MouseMovedWhilePressed;

        /// <summary>
        /// Срабатывает когда ПКМ отпущена
        /// </summary>
        public event Action<double, double>? MouseButtonReleased;

        public MouseHook(double moveThresholdPx = 5.0, int holdThresholdMs = 100)
        {
            _moveThresholdPx = moveThresholdPx;
            _holdThresholdMs = holdThresholdMs;
        }

        /// <summary>
        /// 🔧 Обновляет пороги без пересоздания hook'а.
        /// Это сохраняет подписки на события и Win32 hook остаётся рабочим.
        /// </summary>
        public void UpdateThresholds(double moveThresholdPx, int holdThresholdMs)
        {
            _moveThresholdPx = moveThresholdPx;
            _holdThresholdMs = holdThresholdMs;
            Console.WriteLine($"🔧 MouseHook пороги обновлены: move={moveThresholdPx}px, hold={holdThresholdMs}ms");
        }

        /// <summary>
        /// Устанавливает глобальный хук мыши
        /// </summary>
        public void Start()
        {
            if (_hookId != IntPtr.Zero) return;

            _proc = HookCallback;
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule!)
            {
                _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }

            if (_hookId == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"❌ Не удалось установить хук мыши. Error: {error}");
            }
            else
            {
                Console.WriteLine("✅ Глобальный хук мыши установлен");
            }
        }

        /// <summary>
        /// Обработчик всех событий мыши в системе
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                int msg = wParam.ToInt32();

                switch (msg)
                {
                    case WM_RBUTTONDOWN:
                        HandleRightButtonDown(hookStruct);
                        break;

                    case WM_RBUTTONUP:
                        HandleRightButtonUp(hookStruct);
                        break;

                    case WM_MOUSEMOVE:
                        HandleMouseMove(hookStruct);
                        break;
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void HandleRightButtonDown(MSLLHOOKSTRUCT hookStruct)
        {
            _isRightButtonPressed = true;
            _pressStartTime = DateTime.Now;
            _pressStartPoint = new System.Windows.Point(hookStruct.pt.x, hookStruct.pt.y);
            _lastPoint = _pressStartPoint;
            _isDragging = false;
        }

        private void HandleRightButtonUp(MSLLHOOKSTRUCT hookStruct)
        {
            if (_isRightButtonPressed)
            {
                _isRightButtonPressed = false;

                // Проверяем что это был жест (достаточно долго удерживали и двинули)
                var holdTime = (DateTime.Now - _pressStartTime).TotalMilliseconds;

                if (_isDragging || holdTime >= _holdThresholdMs)
                {
                    // 🔑 КРИТИЧНО: маршалим событие в UI поток!
                    // MouseHook вызывается из Win32 callback (не UI поток)
                    double x = hookStruct.pt.x;
                    double y = hookStruct.pt.y;
                    
                    if (Application.Current?.Dispatcher?.CheckAccess() == true)
                    {
                        MouseButtonReleased?.Invoke(x, y);
                    }
                    else
                    {
                        Application.Current?.Dispatcher?.BeginInvoke(
                            new Action(() => MouseButtonReleased?.Invoke(x, y)),
                            DispatcherPriority.Normal);
                    }
                }

                _isDragging = false;
            }
        }

        private void HandleMouseMove(MSLLHOOKSTRUCT hookStruct)
        {
            if (!_isRightButtonPressed) return;

            var currentPoint = new System.Windows.Point(hookStruct.pt.x, hookStruct.pt.y);
            
            // Проверяем порог движения от начальной точки
            if (!_isDragging)
            {
                double dx = currentPoint.X - _pressStartPoint.X;
                double dy = currentPoint.Y - _pressStartPoint.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance >= _moveThresholdPx)
                {
                    _isDragging = true;
                }
            }

            // Если мы в режиме рисования - отправляем точку
            if (_isDragging)
            {
                double x = hookStruct.pt.x;
                double y = hookStruct.pt.y;

                // 🔑 КРИТИЧНО: маршалим событие в UI поток!
                if (Application.Current?.Dispatcher?.CheckAccess() == true)
                {
                    MouseMovedWhilePressed?.Invoke(x, y);
                }
                else
                {
                    Application.Current?.Dispatcher?.BeginInvoke(
                        new Action(() => MouseMovedWhilePressed?.Invoke(x, y)),
                        DispatcherPriority.Normal);
                }

                _lastPoint = currentPoint;
            }
        }

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
                Console.WriteLine("✅ Хук мыши удалён");
            }
        }

        #region Win32 API

        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion
    }
}