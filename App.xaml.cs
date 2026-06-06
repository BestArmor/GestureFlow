using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

// 🔑 Алиасы чтобы разрешить конфликт между WPF и WinForms
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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
        private TrayIconManager? _trayIcon;
        private bool _isDrawing = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("🚀 Запуск GestureFlow v0.8...");

            _config = new ConfigService();
            var cfg = _config.Current;

            Console.WriteLine($"📊 Пороги: move={cfg.Thresholds.MoveThresholdPx}px, hold={cfg.Thresholds.HoldThresholdMs}ms, min={cfg.Thresholds.MinGestureLengthPx}px");
            Console.WriteLine($"🎨 Трейл: {cfg.Trail.Color}, толщина={cfg.Trail.Thickness}px");

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

            _config.SettingsChanged += OnSettingsChanged;

            // 🎯 Иконка в трее
            _trayIcon = new TrayIconManager();
            _trayIcon.ExitRequested += () => Shutdown();
            _trayIcon.AboutRequested += ShowAboutDialog;
            _trayIcon.OpenSettingsRequested += OpenSettingsFile;

            _hook.Start();

            Console.WriteLine("✅ GestureFlow запущен!");
            Console.WriteLine("🎯 Иконка в системном трее активна");
            Console.WriteLine("💜 Трейл активен");
            Console.WriteLine("👉 Зажми ПКМ и двигай");

            // 💡 Toast-уведомление при запуске
            if (!cfg.General.StartMinimized)
            {
                _trayIcon.ShowNotification(
                    "GestureFlow запущен 💜",
                    "Рисуй жесты с зажатой ПКМ!");
            }
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show(
                "🎨 GestureFlow v0.8.0\n\n" +
                "Управление ПК с помощью жестов мыши\n\n" +
                "══════════════════\n" +
                "🎬 Жесты:\n" +
                "  ← Назад  |  → Вперёд\n" +
                "  ↑ Новая вкладка  |  ↓ Закрыть\n" +
                "  ⭕ Обновить  |  ✅ Свернуть окно\n" +
                "══════════════════\n\n" +
                "⚙️ Конфигурация: settings.json\n" +
                "🔥 Hot-reload: изменения применяются мгновенно\n\n" +
                "© 2026 BestArmor | MIT License",
                "О программе GestureFlow",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OpenSettingsFile()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(configPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = configPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("settings.json не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSettingsChanged(GestureFlow.Models.AppSettings newCfg)
        {
            Console.WriteLine($"🔄 Применяем новые настройки...");

            _recognizer = new GestureRecognizer(newCfg.Thresholds.MinGestureLengthPx);
            _trail?.UpdateSettings(newCfg.Trail.Color, newCfg.Trail.Thickness);

            _hook?.Dispose();
            _hook = new MouseHook(
                moveThresholdPx: newCfg.Thresholds.MoveThresholdPx,
                holdThresholdMs: newCfg.Thresholds.HoldThresholdMs);

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
            Console.WriteLine("✅ Новые пороги применены");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hook?.Dispose();
            _trail?.Dispose();
            _config?.Dispose();
            _trayIcon?.Dispose();
            base.OnExit(e);
        }
    }
}