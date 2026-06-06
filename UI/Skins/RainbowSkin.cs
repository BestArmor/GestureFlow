using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;

namespace GestureFlow.UI.Skins
{
    /// <summary>
    /// 🌈 Rainbow скин — супер-оптимизированный через Polyline + LinearGradientBrush.
    /// Один Polyline с обновляемым градиентом = 60fps без лагов!
    /// </summary>
    public class RainbowSkin : ITrailSkin
    {
        private const int GRADIENT_STOPS = 12; // Количество точек градиента

        private Canvas? _canvas;
        private Polyline? _trailLine;
        private LinearGradientBrush? _gradientBrush;
        private readonly double _thickness;
        private double _hueOffset = 0;

        public string Name => "Rainbow";
        public string Icon => "🌈";

        public RainbowSkin(double thickness = 4.0)
        {
            _thickness = thickness;
        }

        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;

            // Создаём градиентную кисть с GRADIENT_STOPS точками
            _gradientBrush = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0.5),
                EndPoint = new System.Windows.Point(1, 0.5),
                MappingMode = BrushMappingMode.RelativeToBoundingBox
            };

            for (int i = 0; i < GRADIENT_STOPS; i++)
            {
                double offset = (double)i / (GRADIENT_STOPS - 1);
                _gradientBrush.GradientStops.Add(new GradientStop(Colors.White, offset));
            }

            _trailLine = new Polyline
            {
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeThickness = _thickness,
                IsHitTestVisible = false,
                Stroke = _gradientBrush
            };

            _canvas.Children.Add(_trailLine);
        }

        public void Draw(List<Point> points)
        {
            if (_trailLine == null || _gradientBrush == null) return;

            // Обновляем точки линии
            _trailLine.Points = new PointCollection(points);

            // 🌈 Анимация: смещаем спектр
            _hueOffset += 0.02;

            // Обновляем цвета градиента (без создания новых объектов!)
            for (int i = 0; i < GRADIENT_STOPS; i++)
            {
                double hue = ((_hueOffset + (double)i / GRADIENT_STOPS) % 1.0);
                _gradientBrush.GradientStops[i].Color = HsvToRgb(hue, 1.0, 1.0);
            }
        }

        public void Clear()
        {
            _trailLine?.Points.Clear();
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

        private Color HsvToRgb(double h, double s, double v)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = v;
            }
            else
            {
                var i = (int)(h * 6);
                var f = h * 6 - i;
                var p = v * (1 - s);
                var q = v * (1 - f * s);
                var t = v * (1 - (1 - f) * s);

                switch (i % 6)
                {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    default: r = v; g = p; b = q; break;
                }
            }

            return Color.FromRgb(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255));
        }
    }
}