using System;
using System.Collections.Generic;

namespace GestureFlow.Core
{
    public enum GestureType { None, Left, Right, Up, Down }

    public class GestureRecognizer
    {
        private const double MinLengthPx = 30.0; // Уменьшили с 50 до 30
        private readonly List<(double X, double Y)> _points = new();

        public void AddPoint(double x, double y) => _points.Add((x, y));
        public void Reset() => _points.Clear();

        public GestureType Recognize()
        {
            try
            {
                // 🔑 УПРОЩЕНИЕ: достаточно 1 точки движения (было 2)
                if (_points.Count < 1)
                    return GestureType.None;

                var first = _points[0];
                var last = _points[^1];

                double dx = last.X - first.X;
                double dy = last.Y - first.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length < MinLengthPx)
                    return GestureType.None;

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
    }
}