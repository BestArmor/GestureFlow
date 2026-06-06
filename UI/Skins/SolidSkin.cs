using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

// 🔑 Алиасы для разрешения конфликта WPF/WinForms
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace GestureFlow.UI.Skins
{
    /// <summary>
    /// 💜 Solid скин — классический однотонный трейл с glow эффектом.
    /// </summary>
    public class SolidSkin : ITrailSkin
    {
        private Canvas? _canvas;
        private Polyline? _trailLine;
        private readonly string _colorHex;
        private readonly double _thickness;

        public string Name => "Solid";
        public string Icon => "💜";

        public SolidSkin(string colorHex = "#FF50B4", double thickness = 4.0)
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
                IsHitTestVisible = false
            };

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(_colorHex)!;
                _trailLine.Stroke = new SolidColorBrush(color);
                _trailLine.StrokeThickness = _thickness;
                _trailLine.Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
            }
            catch
            {
                var fallbackColor = (Color)ColorConverter.ConvertFromString("#FF50B4")!;
                _trailLine.Stroke = new SolidColorBrush(fallbackColor);
                _trailLine.StrokeThickness = _thickness;
            }

            _canvas.Children.Add(_trailLine);
        }

        public void Draw(List<Point> points)
        {
            if (_trailLine == null) return;
            _trailLine.Points = new PointCollection(points);
        }

        public void Clear()
        {
            if (_trailLine != null)
                _trailLine.Points.Clear();
        }

        public void SetOpacity(double opacity)
        {
            if (_trailLine != null)
                _trailLine.Opacity = opacity;
        }

        public void Dispose()
        {
            if (_canvas != null && _trailLine != null)
                _canvas.Children.Remove(_trailLine);
        }
    }
}