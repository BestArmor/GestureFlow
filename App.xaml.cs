using System;
using System.Windows;
using GestureFlow.Core;
using GestureFlow.Services;
using GestureFlow.UI;

namespace GestureFlow
{
    public partial class App : Application
    {
        private MouseHook? _hook;
        private GestureRecognizer? _recognizer;
        private ActionExecutor? _executor;
        private TrailOverlay? _trail;
        private ConfigService? _config;
        private bool _isDrawing = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("🚀 Запуск GestureFlow v0.7...");

            // 🔧 Инициализируем ConfigService — создаст settings.json если его нет
            _config = new ConfigService();
            var cfg = _config.Current;

            Console.WriteLine($"📊 Пороги: move={cfg.Thresholds.MoveThresholdPx}px, hold={cfg.Thresholds.HoldThresholdMs}ms, min={cfg.Thresholds.MinGestureLengthPx}px");
            Console.WriteLine($"🎨 Трейл: {cfg.Trail.Color}, толщина={cfg.Trail.Thickness}px");

            // 🔧 Передаём пороги из конфига в компоненты
            _recognizer = new GestureRecognizer(cfg.Thresholds.MinGestureLengthPx);
            _executor = new ActionExecutor();
            _trail = new TrailOverlay(cfg.Trail.Color, cfg.Trail.Thickness);
            _hook = new MouseHook(
                moveThresholdPx: cfg.Thresholds.MoveThresholdPx,
                holdThresholdMs: cfg.Thresholds.HoldThresholdMs);

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

            // 🔧 Подписываемся на hot-reload настроек
            _config.SettingsChanged += OnSettingsChanged;

            _hook.Start();

            Console.WriteLine("✅ GestureFlow запущен!");
            Console.WriteLine("💜 Трейл активен");
            Console.WriteLine("📝 settings.json создан — можешь редактировать");
            Console.WriteLine("👉 Зажми ПКМ и двигай");
        }

        /// <summary>
        /// 🔥 Hot-reload: когда settings.json меняется, пересоздаём компоненты с новыми порогами
        /// </summary>
        private void OnSettingsChanged(GestureFlow.Models.AppSettings newCfg)
        {
            Console.WriteLine($"🔄 Применяем новые настройки...");
            
            // Пересоздаём распознаватель с новой минимальной длиной
            _recognizer = new GestureRecognizer(newCfg.Thresholds.MinGestureLengthPx);
            
            // Пересоздаём хук с новыми порогами
            _hook?.Dispose();
            _hook = new MouseHook(
                moveThresholdPx: newCfg.Thresholds.MoveThresholdPx,
                holdThresholdMs: newCfg.Thresholds.HoldThresholdMs);

            // Переподписываем события
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
                    _trail!.EndDrawing();
                    _isDrawing = false;
                }
            };

            _hook.Start();
            // 🔥 Обновляем внешний вид трейла
            _trail?.UpdateSettings(newCfg.Trail.Color, newCfg.Trail.Thickness);
            Console.WriteLine("✅ Новые пороги применены");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hook?.Dispose();
            _trail?.Dispose();
            _config?.Dispose();
            base.OnExit(e);
        }
    }
}