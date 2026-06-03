using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

// Алиасы для будущих расширений (WinForms)
using WpfPoint = System.Windows.Point;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;

namespace GestureFlow.UI
{
    /// <summary>
    /// 💜 Прозрачное окно поверх всего экрана, рисующее фиолетовый трейл жеста.
    /// - IsHitTestVisible = false → клики мыши проходят сквозь окно
    /// - Поддержка нескольких мониторов (VirtualScreen)
    /// - Плавное затухание после отпускания ПКМ
    /// </summary>
    public class TrailOverlay : IDisposable
    {
        private const int MAX_POINTS = 300;
        private const double FADE_STEP = 0.15;
        private const int FADE_INTERVAL_MS = 30;
        private const string TRAIL_COLOR = "#FF50B4";
        private const double TRAIL_THICKNESS = 4.0;

        private Window? _window;
        private Polyline? _trailLine;
        private DispatcherTimer? _fadeTimer;
        private readonly List<WpfPoint> _buffer = new();
        private double _opacity = 1.0;
        private bool _isVisible = false;

        public TrailOverlay()
        {
            CreateWindow();
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
                IsHitTestVisible = false,  // 🔑 КЛИКИ ПРОХОДЯТ СКВОЗЬ!
                
                // 🖥️ Поддержка нескольких мониторов
                Left = SystemParameters.VirtualScreenLeft,
                Top = SystemParameters.VirtualScreenTop,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                
                Opacity = 0
            };

            var canvas = new Canvas
            {
                Background = WpfBrushes.Transparent,
                IsHitTestVisible = false
            };

            var color = (WpfColor)WpfColorConverter.ConvertFromString(TRAIL_COLOR)!;

            _trailLine = new Polyline
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = TRAIL_THICKNESS,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false,
                Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.8
                }
            };

            canvas.Children.Add(_trailLine);
            _window.Content = canvas;

            _fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(FADE_INTERVAL_MS)
            };
            _fadeTimer.Tick += FadeTimer_Tick;

            // Показываем окно невидимым
            _window.Show();
        }

        /// <summary>
        /// Начинает рисование жеста
        /// </summary>
        public void StartDrawing(double x, double y)
        {
            if (_window == null) return;

            _buffer.Clear();
            _trailLine!.Points.Clear();
            _opacity = 1.0;
            _trailLine.Opacity = 1.0;
            _fadeTimer?.Stop();

            _window.Opacity = 1;
            _isVisible = true;

            AddPoint(x, y);
        }

        /// <summary>
        /// Добавляет точку к трейлу
        /// </summary>
        public void AddPoint(double x, double y)
        {
            if (_window == null || !_isVisible) return;

            // Конвертируем экранные координаты в локальные (для много-мониторных систем)
            double localX = x - SystemParameters.VirtualScreenLeft;
            double localY = y - SystemParameters.VirtualScreenTop;
            _buffer.Add(new WpfPoint(localX, localY));

            // Ограничиваем количество точек
            if (_buffer.Count > MAX_POINTS)
            {
                var thinned = new List<WpfPoint>();
                int step = _buffer.Count / MAX_POINTS + 1;
                for (int i = 0; i < _buffer.Count; i += step)
                    thinned.Add(_buffer[i]);
                thinned.Add(_buffer[^1]);
                _buffer.Clear();
                _buffer.AddRange(thinned);
            }

            _trailLine!.Points = new PointCollection(_buffer);
        }

        /// <summary>
        /// Завершает рисование — трейл начнёт плавно исчезать
        /// </summary>
        public void EndDrawing()
        {
            _isVisible = false;
            _fadeTimer?.Start();
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (_trailLine == null || _window == null) return;

            _opacity -= FADE_STEP;
            _trailLine.Opacity = Math.Max(0, _opacity);

            if (_opacity <= 0)
            {
                _fadeTimer?.Stop();
                _trailLine.Points.Clear();
                _buffer.Clear();
                _window.Opacity = 0;
            }
        }

        public void Dispose()
        {
            _fadeTimer?.Stop();
            _window?.Close();
        }
    }
}