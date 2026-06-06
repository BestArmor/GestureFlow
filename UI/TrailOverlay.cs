using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

// 🔑 Алиасы WPF
using Application = System.Windows.Application;
using WpfPoint = System.Windows.Point;
using WpfBrushes = System.Windows.Media.Brushes;

using GestureFlow.Models;
using GestureFlow.Services;
using GestureFlow.UI.Skins;

namespace GestureFlow.UI
{
    /// <summary>
    /// 💜 Trail Overlay с правильной синхронизацией между потоками.
    /// Producer-Consumer + lock-free skin swap.
    /// </summary>
    public class TrailOverlay : IDisposable
    {
        private const int MAX_POINTS = 300;
        private const double FADE_STEP = 0.05;

        private Window? _window;
        private Canvas? _canvas;
        private readonly List<WpfPoint> _points = new();
        private readonly object _pointsLock = new();

        private double _opacity = 0.0;
        private bool _isDrawing = false;
        private bool _needsClear = false;

        private ITrailSkin? _currentSkin;
        private readonly SkinManager _skinManager;
        private readonly object _skinLock = new();

        public TrailOverlay(SkinManager skinManager, TrailSettings settings)
        {
            _skinManager = skinManager;
            CreateWindow();
            SetSkin(settings.Skin, settings);
            CompositionTarget.Rendering += OnRenderFrame;
        }

        /// <summary>
        /// 🎬 Каждый кадр UI потока - рисуем трейл
        /// </summary>
        private void OnRenderFrame(object? sender, EventArgs e)
        {
            if (_window == null || _canvas == null) return;

            // Если окно невидимо - не рендерим
            if (_window.Opacity <= 0 && !_isDrawing) return;

            ITrailSkin? skin;
            bool isDrawing;
            bool needsClear;
            double opacity;
            List<WpfPoint> pointsCopy;

            // Короткий lock только для чтения состояния
            lock (_skinLock)
            {
                skin = _currentSkin;
            }

            if (skin == null) return;

            lock (_pointsLock)
            {
                isDrawing = _isDrawing;
                needsClear = _needsClear;
                opacity = _opacity;
                _needsClear = false;
                pointsCopy = new List<WpfPoint>(_points);
            }

            // 🎨 Вне lock: очистка
            if (needsClear)
            {
                try { skin.Clear(); } catch { }
            }

            // Если ничего не рисуем и прозрачность 0 - выходим
            if (!isDrawing && opacity <= 0) return;

            // 🎨 Вне lock: отрисовка
            if (pointsCopy.Count > 0)
            {
                try
                {
                    skin.Draw(pointsCopy);
                    skin.SetOpacity(opacity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Render error: {ex.Message}");
                }
            }

            // Fade-out
            if (!isDrawing && opacity > 0)
            {
                double newOpacity = opacity - FADE_STEP;
                lock (_pointsLock)
                {
                    _opacity = newOpacity;
                }

                if (newOpacity <= 0)
                {
                    lock (_pointsLock)
                    {
                        _points.Clear();
                        _opacity = 0;
                    }
                    try { skin.Clear(); } catch { }
                    _window.Opacity = 0;
                }
            }
        }

        /// <summary>
        /// 🔥 Меняет скин БЕЗ deadlock'ов.
        /// 1. Создаём новый скин ВНЕ lock
        /// 2. Атомарно меняем ссылку
        /// 3. Dispose старого ВНЕ lock
        /// </summary>
        public void SetSkin(string skinName, TrailSettings settings)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == false)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => SetSkin(skinName, settings)),
                    DispatcherPriority.Render);
                return;
            }

            try
            {
                // Шаг 1: Создаём новый скин ВНЕ lock (медленная операция)
                var newSkin = _skinManager.CreateSkinInstance(skinName, settings);
                if (_canvas != null)
                    newSkin.Initialize(_canvas);

                // Шаг 2: Атомарная замена ссылки под lock (быстрая)
                ITrailSkin? oldSkin;
                lock (_skinLock)
                {
                    oldSkin = _currentSkin;
                    _currentSkin = newSkin;
                }

                // Шаг 3: Dispose старого ВНЕ lock
                if (oldSkin != null)
                {
                    try { oldSkin.Dispose(); } catch { }
                }

                Console.WriteLine($"🎨 Скин: {newSkin.Icon} {newSkin.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка смены скина: {ex.Message}");
            }
        }

        public void UpdateSettings(TrailSettings settings)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == false)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => UpdateSettings(settings)),
                    DispatcherPriority.Render);
                return;
            }

            string? currentName;
            lock (_skinLock) currentName = _currentSkin?.Name;
            if (currentName != null)
                SetSkin(currentName, settings);
        }

        private void CreateWindow()
        {
            _window = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = WpfBrushes.Transparent,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                ShowActivated = false,
                Focusable = false,
                IsHitTestVisible = false,
                Left = SystemParameters.VirtualScreenLeft,
                Top = SystemParameters.VirtualScreenTop,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                Opacity = 0
            };

            _canvas = new Canvas
            {
                Background = WpfBrushes.Transparent,
                IsHitTestVisible = false
            };

            _window.Content = _canvas;
            _window.Show();
        }

        public void StartDrawing(double x, double y)
        {
            lock (_pointsLock)
            {
                _points.Clear();
                _isDrawing = true;
                _opacity = 1.0;
                _needsClear = true;
            }
            if (_window != null) _window.Opacity = 1;
            AddPoint(x, y);
        }

        public void AddPoint(double x, double y)
        {
            double localX = x - SystemParameters.VirtualScreenLeft;
            double localY = y - SystemParameters.VirtualScreenTop;

            lock (_pointsLock)
            {
                _points.Add(new WpfPoint(localX, localY));

                if (_points.Count > MAX_POINTS)
                {
                    var thinned = new List<WpfPoint>();
                    int step = _points.Count / MAX_POINTS + 1;
                    for (int i = 0; i < _points.Count; i += step)
                        thinned.Add(_points[i]);
                    thinned.Add(_points[^1]);
                    _points.Clear();
                    _points.AddRange(thinned);
                }
            }
        }

        public void EndDrawing()
        {
            lock (_pointsLock)
            {
                _isDrawing = false;
            }
        }

        public void Dispose()
        {
            try
            {
                CompositionTarget.Rendering -= OnRenderFrame;
                ITrailSkin? skin;
                lock (_skinLock)
                {
                    skin = _currentSkin;
                    _currentSkin = null;
                }
                try { skin?.Dispose(); } catch { }
                _window?.Close();
            }
            catch { }
        }
    }
}