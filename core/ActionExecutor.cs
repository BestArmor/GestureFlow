using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GestureFlow.Core;

namespace GestureFlow.Core
{
    /// <summary>
    /// ⌨️ Выполняет действия через SendInput API + прямые Win32 вызовы.
    /// </summary>
    public class ActionExecutor
    {
        private static bool _isCooldown = false;
        private static readonly object _lock = new();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // 🔑 Win32 API для управления окнами (галочка)
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MINIMIZE = 6;

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYDOWN = 0;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void Execute(GestureType gesture)
        {
            lock (_lock)
            {
                if (_isCooldown) return;
                _isCooldown = true;
            }
            Task.Run(() => { Thread.Sleep(1000); lock (_lock) _isCooldown = false; });

            switch (gesture)
            {
                case GestureType.Left:
                    Console.WriteLine("⌨️ Выполняю: Alt+Left");
                    SendCombo("Alt+Left");
                    break;
                case GestureType.Right:
                    Console.WriteLine("⌨️ Выполняю: Alt+Right");
                    SendCombo("Alt+Right");
                    break;
                case GestureType.Up:
                    Console.WriteLine("⌨️ Выполняю: Ctrl+T");
                    SendCombo("Ctrl+T");
                    break;
                case GestureType.Down:
                    Console.WriteLine("⌨️ Выполняю: Ctrl+W");
                    SendCombo("Ctrl+W");
                    break;
                case GestureType.Circle:
                    Console.WriteLine("⌨️ Выполняю: F5 (обновить)");
                    SendCombo("F5");
                    break;
                case GestureType.Checkmark:
                    // 🔑 ПРЯМОЙ Win32 вызов — 100% сворачивает окно в любом режиме!
                    Console.WriteLine("⌨️ Сворачиваю активное окно (ShowWindow)");
                    MinimizeActiveWindow();
                    break;
                default:
                    Console.WriteLine($"⚠️ Нет действия для {gesture}");
                    break;
            }
        }

        /// <summary>
        /// 🔑 Сворачивает активное окно через Win32 API.
        /// Работает всегда: fullscreen, maximized, restored — без разницы.
        /// </summary>
        private void MinimizeActiveWindow()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                {
                    Console.WriteLine("❌ Нет активного окна");
                    return;
                }

                bool result = ShowWindow(hwnd, SW_MINIMIZE);
                Console.WriteLine(result ? "✅ Окно свёрнуто" : "❌ ShowWindow failed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MinimizeActiveWindow error: {ex.Message}");
            }
        }

        private void SendCombo(string combo)
        {
            try
            {
                var parts = combo.Split('+', StringSplitOptions.RemoveEmptyEntries);
                var keys = new List<ushort>();
                foreach (var p in parts)
                {
                    ushort vk = ParseKey(p.Trim());
                    if (vk != 0) keys.Add(vk);
                }

                if (keys.Count == 0) return;

                var inputs = new INPUT[keys.Count * 2];

                // KeyDown
                for (int i = 0; i < keys.Count; i++)
                {
                    inputs[i] = new INPUT
                    {
                        type = INPUT_KEYBOARD,
                        U = new InputUnion
                        {
                            ki = new KEYBDINPUT
                            {
                                wVk = keys[i],
                                wScan = 0,
                                dwFlags = KEYEVENTF_KEYDOWN,
                                time = 0,
                                dwExtraInfo = IntPtr.Zero
                            }
                        }
                    };
                }

                // KeyUp (в обратном порядке)
                for (int i = 0; i < keys.Count; i++)
                {
                    inputs[keys.Count + i] = new INPUT
                    {
                        type = INPUT_KEYBOARD,
                        U = new InputUnion
                        {
                            ki = new KEYBDINPUT
                            {
                                wVk = keys[keys.Count - 1 - i],
                                wScan = 0,
                                dwFlags = KEYEVENTF_KEYUP,
                                time = 0,
                                dwExtraInfo = IntPtr.Zero
                            }
                        }
                    };
                }

                int size = Marshal.SizeOf(typeof(INPUT));
                uint result = SendInput((uint)inputs.Length, inputs, size);

                if (result == 0)
                {
                    int err = Marshal.GetLastWin32Error();
                    Console.WriteLine($"❌ SendInput failed: {err}");
                }
                else
                {
                    Console.WriteLine($"✅ OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SendCombo error: {ex.Message}");
            }
        }

        private ushort ParseKey(string k) => k.Trim().ToLowerInvariant() switch
        {
            "ctrl" or "control" => 0x11,
            "shift" => 0x10,
            "alt" or "menu" => 0x12,
            "win" or "windows" or "super" => 0x5B,
            "left" => 0x25,
            "right" => 0x27,
            "up" => 0x26,
            "down" => 0x28,
            "t" => 0x54,
            "w" => 0x57,
            "f4" => 0x73,
            "f5" => 0x74,
            _ => 0
        };

        // === Win32 структуры ===

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
    }
}