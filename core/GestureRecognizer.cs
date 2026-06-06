using System;
using System.Collections.Generic;
using System.Linq;

namespace GestureFlow.Core
{
    public class GestureRecognizer
    {
        // 🔧 Теперь настраивается через конструктор
        private readonly double _minLengthPx;
        private readonly List<(double X, double Y)> _points = new();

        /// <summary>
        /// Создаёт распознаватель жестов.
        /// </summary>
        /// <param name="minGestureLengthPx">Минимальная длина жеста в пикселях (из settings.json)</param>
        public GestureRecognizer(double minGestureLengthPx = 50.0)
        {
            _minLengthPx = minGestureLengthPx;
        }

        public void AddPoint(double x, double y) => _points.Add((x, y));
        public void Reset() => _points.Clear();

        public GestureType Recognize()
        {
            try
            {
                if (_points.Count < 5) return GestureType.None;

                var first = _points[0];
                var last = _points[^1];
                double dx = last.X - first.X;
                double dy = last.Y - first.Y;
                double straightLength = Math.Sqrt(dx * dx + dy * dy);

                if (straightLength < _minLengthPx) return GestureType.None;

                double pathLength = PathLength(_points);
                double width = _points.Max(p => p.X) - _points.Min(p => p.X);
                double height = _points.Max(p => p.Y) - _points.Min(p => p.Y);

                // ⭕ КРУГ
                double closedness = straightLength / pathLength;
                double boxRatio = Math.Min(width, height) / (Math.Max(width, height) + 0.001);
                double boxPerimeter = 2 * (width + height);
                if (boxPerimeter < 0.001) boxPerimeter = 0.001;
                double pathToBox = pathLength / boxPerimeter;

                if (closedness < 0.35 && boxRatio > 0.6 && pathToBox > 0.7 && width > 60 && height > 60)
                {
                    Console.WriteLine($"⭕ Круг (замкнутость={closedness:F2}, ratio={boxRatio:F2})");
                    return GestureType.Circle;
                }

                // ✅ ГАЛОЧКА
                int bottomIdx = 0;
                double maxY = double.MinValue;
                for (int i = 0; i < _points.Count; i++)
                {
                    if (_points[i].Y > maxY)
                    {
                        maxY = _points[i].Y;
                        bottomIdx = i;
                    }
                }

                if (bottomIdx > _points.Count * 0.2 && bottomIdx < _points.Count * 0.85)
                {
                    double descent = _points[bottomIdx].Y - _points[0].Y;
                    double ascent = _points[bottomIdx].Y - _points[^1].Y;
                    double horizontalProgress = _points[^1].X - _points[0].X;

                    if (descent > 40 && ascent > 40 && horizontalProgress > 30)
                    {
                        Console.WriteLine($"✅ Галочка (вниз={descent:F0}, вверх={ascent:F0}, вправо={horizontalProgress:F0})");
                        return GestureType.Checkmark;
                    }
                }

                // Fallback: направление
                if (Math.Abs(dx) > Math.Abs(dy))
                    return dx < 0 ? GestureType.Left : GestureType.Right;
                else
                    return dy < 0 ? GestureType.Up : GestureType.Down;
            }
            finally
            {
                _points.Clear();
            }
        }

        private double PathLength(List<(double X, double Y)> points)
        {
            double len = 0;
            for (int i = 1; i < points.Count; i++)
            {
                double dx = points[i].X - points[i - 1].X;
                double dy = points[i].Y - points[i - 1].Y;
                len += Math.Sqrt(dx * dx + dy * dy);
            }
            return len;
        }
    }
}