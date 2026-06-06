using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace GestureFlow.UI.Skins
{
    /// <summary>
    /// 💫 Blink скин — оптимизированный (используем глобальное время).
    /// </summary>
    public class BlinkSkin : ITrailSkin
    {
        private Canvas? _canvas;
        private Polyline? _trailLine;
        private double _baseOpacity = 1.0;
        private readonly string _colorHex;
        private readonly double _thickness;
        private readonly DateTime _startTime = DateTime.Now;

        public string Name => "Blink";
        public string Icon => "💫";

        public BlinkSkin(string colorHex = "#FF50B4", double thickness = 4.0)
        {
            _colorHex = colorHex;
            _thickness = thickness;
        }

        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;
            _trailLine = new Polyline
            {
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false,
                StrokeThickness = _thickness
            };

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(_colorHex)!;
                _trailLine.Stroke = new SolidColorBrush(color);
                _trailLine.Effect = new DropShadowEffect
                {
                    Color = color, BlurRadius = 15, ShadowDepth = 0, Opacity = 1
                };
            }
            catch
            {
                var fallback = (Color)ColorConverter.ConvertFromString("#FF50B4")!;
                _trailLine.Stroke = new SolidColorBrush(fallback);
            }

            _canvas.Children.Add(_trailLine);
        }

        public void Draw(List<Point> points)
        {
            if (_trailLine == null) return;
            _trailLine.Points = new PointCollection(points);

            // 🔥 Используем глобальное время вместо таймера
            var elapsed = (DateTime.Now - _startTime).TotalMilliseconds;
            double pulse = 0.65 + 0.35 * Math.Sin(elapsed * 0.01);
            _trailLine.Opacity = _baseOpacity * pulse;
        }

        public void Clear()
        {
            _trailLine?.Points.Clear();
        }

        public void SetOpacity(double opacity)
        {
            _baseOpacity = opacity;
        }

        public void Dispose()
        {
            if (_canvas != null && _trailLine != null)
                _canvas.Children.Remove(_trailLine);
        }
    }
}