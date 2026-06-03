using System;
using System.Windows;
using GestureFlow.Core;
using GestureFlow.UI;

namespace GestureFlow
{
    public partial class App : Application
    {
        private MouseHook? _hook;
        private GestureRecognizer? _recognizer;
        private ActionExecutor? _executor;
        private TrailOverlay? _trail;
        private bool _isDrawing = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("🚀 Запуск GestureFlow...");

            _recognizer = new GestureRecognizer();
            _executor = new ActionExecutor();
            _trail = new TrailOverlay();
            
            // 🔑 Уменьшенные пороги для быстрых жестов
            _hook = new MouseHook(moveThresholdPx: 5.0, holdThresholdMs: 100);

            _hook.MouseMovedWhilePressed += (x, y) =>
            {
                if (!_isDrawing)
                {
                    _isDrawing = true;
                    _recognizer!.Reset();
                    _trail!.StartDrawing(x, y);
                }
                _recognizer!.AddPoint(x, y);
                _trail!.AddPoint(x, y);
            };

            _hook.MouseButtonReleased += (x, y) =>
            {
                if (_isDrawing)
                {
                    var gesture = _recognizer!.Recognize();
                    if (gesture != GestureType.None)
                    {
                        Console.WriteLine($"✨ Распознано: {gesture}");
                        _executor!.Execute(gesture);
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Жест слишком короткий");
                    }
                    _trail!.EndDrawing();
                    _isDrawing = false;
                }
            };

            _hook.Start();

            Console.WriteLine("✅ GestureFlow запущен!");
            Console.WriteLine("💜 Трейл активен | Пороги: 5px / 100ms");
            Console.WriteLine("👉 Зажми ПКМ и двигай");
            Console.WriteLine("🛑 Ctrl+C для остановки");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hook?.Dispose();
            _trail?.Dispose();
            base.OnExit(e);
        }
    }
}