using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using GestureFlow.Core;
using GestureFlow.Services;
using GestureFlow.UI;
using GestureFlow.Models;

namespace GestureFlow
{
    public partial class App : System.Windows.Application
    {
        private MouseHook? _hook;
        private GestureRecognizer? _recognizer;
        private ActionExecutor? _executor;
        private TrailOverlay? _trail;
        private ConfigService? _config;
        private TrayIconManager? _trayIcon;
        private SkinManager? _skinManager;
        private MainWindow? _mainWindow;
        private bool _isDrawing = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("🚀 Запуск GestureFlow v0.9.5...");

            _config = new ConfigService();
            var cfg = _config.Current;

            _skinManager = new SkinManager();
            Console.WriteLine($"🎨 Загружено скинов: {_skinManager.Skins.Count}");

            _recognizer = new GestureRecognizer(cfg.Thresholds.MinGestureLengthPx);
            _executor = new ActionExecutor();
            _trail = new TrailOverlay(_skinManager, cfg.Trail);
            _hook = new MouseHook(
                moveThresholdPx: cfg.Thresholds.MoveThresholdPx,
                holdThresholdMs: cfg.Thresholds.HoldThresholdMs);

            _hook.MouseMovedWhilePressed += (x, y) =>
            {
                if (!_isDrawing)
                {
                    _isDrawing = true;
                    _recognizer?.Reset();
                    _trail?.StartDrawing(x, y);
                }
                _recognizer?.AddPoint(x, y);
                _trail?.AddPoint(x, y);
            };

            _hook.MouseButtonReleased += (x, y) =>
            {
                if (_isDrawing)
                {
                    var gesture = _recognizer?.Recognize() ?? GestureType.None;
                    if (gesture != GestureType.None)
                    {
                        Console.WriteLine($"✨ Распознано: {gesture}");
                        _executor?.Execute(gesture);
                    }
                    _trail?.EndDrawing();
                    _isDrawing = false;
                }
            };

            _config.SettingsChanged += OnSettingsChanged;

            _trayIcon = new TrayIconManager();
            _trayIcon.ExitRequested += () => Shutdown();
            _trayIcon.AboutRequested += ShowMainWindow;
            _trayIcon.OpenSettingsRequested += OpenSettingsFile;

            _hook.Start();
            Console.WriteLine("✅ GestureFlow запущен!");
        }

        private void ShowMainWindow()
        {
            if (Current?.Dispatcher?.CheckAccess() == false)
            {
                Current.Dispatcher.Invoke(ShowMainWindow);
                return;
            }

            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow(_config, _skinManager);
                _mainWindow.Closed += (s, e) => _mainWindow = null;
            }

            if (_mainWindow.IsVisible)
            {
                if (_mainWindow.WindowState == WindowState.Minimized)
                    _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
            else
            {
                _mainWindow.Show();
            }
        }

        private void OpenSettingsFile()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(configPath))
                {
                    Process.Start(new ProcessStartInfo { FileName = configPath, UseShellExecute = true });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private void OnSettingsChanged(AppSettings newCfg)
        {
            if (Current?.Dispatcher?.CheckAccess() == false)
            {
                Current.Dispatcher.BeginInvoke(new Action(() => OnSettingsChanged(newCfg)));
                return;
            }

            _recognizer = new GestureRecognizer(newCfg.Thresholds.MinGestureLengthPx);
            _trail?.SetSkin(newCfg.Trail.Skin, newCfg.Trail);
            _hook?.UpdateThresholds(newCfg.Thresholds.MoveThresholdPx, newCfg.Thresholds.HoldThresholdMs);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hook?.Dispose();
            _trail?.Dispose();
            _config?.Dispose();
            _trayIcon?.Dispose();
            _mainWindow?.Close();
            base.OnExit(e);
        }
    }
}