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
    /// ✨ Particle скин — оптимизированный (реже рисуем частицы).
    /// </summary>
    public class ParticleSkin : ITrailSkin
    {
        private const int MAX_PARTICLES = 50; // Уменьшили количество частиц

        private Canvas? _canvas;
        private readonly List<Ellipse> _particles = new();
        private readonly string _colorHex;
        private readonly double _thickness;

        public string Name => "Particle";
        public string Icon => "✨";

        public ParticleSkin(string colorHex = "#FF50B4", double thickness = 4.0)
        {
            _colorHex = colorHex;
            _thickness = thickness;
        }

        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(List<Point> points)
        {
            if (_canvas == null) return;
            Clear();

            Color color;
            try { color = (Color)ColorConverter.ConvertFromString(_colorHex)!; }
            catch { color = (Color)ColorConverter.ConvertFromString("#FF50B4")!; }

            var brush = new SolidColorBrush(color);
            var glow = new DropShadowEffect { Color = color, BlurRadius = 10, ShadowDepth = 0, Opacity = 0.9 };

            // 🔥 Рисуем частицу каждые 8 точек (вместо 3)
            int particleCount = 0;
            for (int i = 0; i < points.Count && particleCount < MAX_PARTICLES; i += 8)
            {
                double size = _thickness + (i % 2 == 0 ? 2 : 0);
                var particle = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = brush,
                    Effect = glow,
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(particle, points[i].X - size / 2);
                Canvas.SetTop(particle, points[i].Y - size / 2);

                _particles.Add(particle);
                _canvas.Children.Add(particle);
                particleCount++;
            }
        }

        public void Clear()
        {
            if (_canvas == null) return;
            foreach (var p in _particles)
                _canvas.Children.Remove(p);
            _particles.Clear();
        }

        public void SetOpacity(double opacity)
        {
            foreach (var p in _particles)
                p.Opacity = opacity;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}