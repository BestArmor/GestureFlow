using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GestureFlow.Core
{
    /// <summary>
    /// ⌨️ Выполняет действия (отправляет комбинации клавиш) через SendInput API.
    /// Работает в фоновом потоке, не блокируя хук мыши.
    /// </summary>
    public class ActionExecutor
    {
        private static bool _isCooldown = false;
        private static readonly object _lock = new();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

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

            string? combo = gesture switch
            {
                GestureType.Left => "Alt+Left",
                GestureType.Right => "Alt+Right",
                GestureType.Up => "Ctrl+T",
                GestureType.Down => "Ctrl+W",
                _ => null
            };

            if (string.IsNullOrEmpty(combo))
            {
                Console.WriteLine($"⚠️ Нет действия для {gesture}");
                return;
            }

            Console.WriteLine($"⌨️ Выполняю: {combo}");
            SendCombo(combo);
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
            "left" => 0x25,
            "right" => 0x27,
            "up" => 0x26,
            "down" => 0x28,
            "t" => 0x54,
            "w" => 0x57,
            "f5" => 0x74,
            _ => 0
        };

        // === Win32 структуры (полные, для 64-bit Windows) ===
        
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