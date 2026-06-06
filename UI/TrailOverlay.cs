using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

using WpfPoint = System.Windows.Point;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;

namespace GestureFlow.UI
{
    /// <summary>
    /// 💜 Прозрачное окно поверх всего экрана, рисующее трейл жеста.
    /// </summary>
    public class TrailOverlay : IDisposable
    {
        private const int MAX_POINTS = 300;
        private const double FADE_STEP = 0.15;
        private const int FADE_INTERVAL_MS = 30;

        private Window? _window;
        private Polyline? _trailLine;
        private DispatcherTimer? _fadeTimer;
        private readonly List<WpfPoint> _buffer = new();
        private double _opacity = 1.0;
        private bool _isVisible = false;

        // 🔧 Настраиваемые параметры
        private string _colorHex;
        private double _thickness;
        private SolidColorBrush? _brush;
        private DropShadowEffect? _glowEffect;

        public TrailOverlay(string colorHex = "#FF50B4", double thickness = 4.0)
        {
            _colorHex = colorHex;
            _thickness = thickness;
            CreateWindow();
        }

        /// <summary>
        /// 🔥 Hot-reload: обновляет цвет и толщину без пересоздания окна
        /// </summary>
        public void UpdateSettings(string colorHex, double thickness)
        {
            _colorHex = colorHex;
            _thickness = thickness;
            ApplyBrushAndEffect();
            Console.WriteLine($"🎨 Трейл обновлён: {colorHex}, {thickness}px");
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

            var canvas = new Canvas
            {
                Background = WpfBrushes.Transparent,
                IsHitTestVisible = false
            };

            _trailLine = new Polyline
            {
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            };

            // 🔧 Применяем цвет и толщину
            ApplyBrushAndEffect();

            canvas.Children.Add(_trailLine);
            _window.Content = canvas;

            _fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(FADE_INTERVAL_MS)
            };
            _fadeTimer.Tick += FadeTimer_Tick;

            _window.Show();
        }

        /// <summary>
        /// Применяет текущие цвет и толщину к трейлу
        /// </summary>
        private void ApplyBrushAndEffect()
        {
            try
            {
                var color = (WpfColor)WpfColorConverter.ConvertFromString(_colorHex)!;
                _brush = new SolidColorBrush(color);
                _glowEffect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };

                if (_trailLine != null)
                {
                    _trailLine.Stroke = _brush;
                    _trailLine.StrokeThickness = _thickness;
                    _trailLine.Effect = _glowEffect;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Неверный цвет '{_colorHex}': {ex.Message}. Используем фиолетовый.");
                var fallbackColor = (WpfColor)WpfColorConverter.ConvertFromString("#FF50B4")!;
                _brush = new SolidColorBrush(fallbackColor);
                if (_trailLine != null)
                {
                    _trailLine.Stroke = _brush;
                    _trailLine.Effect = new DropShadowEffect
                    {
                        Color = fallbackColor,
                        BlurRadius = 10,
                        ShadowDepth = 0,
                        Opacity = 0.8
                    };
                }
            }
        }

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

        public void AddPoint(double x, double y)
        {
            if (_window == null || !_isVisible) return;

            double localX = x - SystemParameters.VirtualScreenLeft;
            double localY = y - SystemParameters.VirtualScreenTop;
            _buffer.Add(new WpfPoint(localX, localY));

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