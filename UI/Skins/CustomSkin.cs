using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

// 🔑 WPF Aliases
using Point = System.Windows.Point;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;
using WpfImage = System.Windows.Controls.Image;

// 🔑 GDI+ Aliases
using GdiBitmap = System.Drawing.Bitmap;
using GdiGraphics = System.Drawing.Graphics;
using GdiColor = System.Drawing.Color;
using GdiFont = System.Drawing.Font;
using GdiBrush = System.Drawing.SolidBrush;
using GdiStringFormat = System.Drawing.StringFormat;
using GdiAlignment = System.Drawing.StringAlignment;
using GdiRect = System.Drawing.Rectangle;
using GdiRectF = System.Drawing.RectangleF;
using GdiPath = System.Drawing.Drawing2D.GraphicsPath;
using GdiPathBrush = System.Drawing.Drawing2D.PathGradientBrush;
using GdiSmoothing = System.Drawing.Drawing2D.SmoothingMode;
using GdiTextHint = System.Drawing.Text.TextRenderingHint;
using GdiPixelFmt = System.Drawing.Imaging.PixelFormat;

namespace GestureFlow.UI.Skins
{
    public class CustomSkin : ITrailSkin
    {
        private const int MAX_CUSTOM_EMOJIS = 100;
        private const double EMOJI_SPACING_PX = 50.0;
        private const int EMOJI_BITMAP_SIZE = 64;
        private const double BASE_EMOJI_SIZE = 32.0;

        private Canvas? _canvas;
        private readonly List<object> _elements = new();
        private readonly CustomSkinConfig _config;

        private readonly WpfImage[] _emojiPool = new WpfImage[MAX_CUSTOM_EMOJIS];
        private readonly string[] _assignedEmojis = new string[MAX_CUSTOM_EMOJIS];
        private readonly double[] _assignedSizes = new double[MAX_CUSTOM_EMOJIS];
        private readonly double[] _assignedRotations = new double[MAX_CUSTOM_EMOJIS];
        private readonly Dictionary<string, ImageSource> _emojiBitmapCache = new();
        private int _activeEmojis = 0;
        private readonly Random _rnd = new();

        public string Name => _config.Name;
        public string Icon => _config.Icon;

        public CustomSkin(CustomSkinConfig config)
        {
            _config = config;

            for (int i = 0; i < MAX_CUSTOM_EMOJIS; i++)
            {
                _emojiPool[i] = new WpfImage
                {
                    Width = BASE_EMOJI_SIZE, Height = BASE_EMOJI_SIZE,
                    Stretch = Stretch.Uniform,
                    IsHitTestVisible = false,
                    Visibility = Visibility.Collapsed,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                _assignedEmojis[i] = "";
            }

            if (_config.Type.ToLowerInvariant() == "emoji" && _config.Emojis != null)
            {
                foreach (var emoji in _config.Emojis.Distinct())
                {
                    if (!_emojiBitmapCache.ContainsKey(emoji))
                    {
                        var source = TryRenderColorEmoji(emoji);
                        if (source != null && HasColor(source))
                            _emojiBitmapCache[emoji] = source;
                        else
                            _emojiBitmapCache[emoji] = CreateColorfulFallback(emoji);
                    }
                }
            }
        }

        private ImageSource? TryRenderColorEmoji(string emoji)
        {
            using var bmp = new GdiBitmap(EMOJI_BITMAP_SIZE, EMOJI_BITMAP_SIZE, GdiPixelFmt.Format32bppArgb);
            using (var g = GdiGraphics.FromImage(bmp))
            {
                g.SmoothingMode = GdiSmoothing.AntiAlias;
                g.TextRenderingHint = GdiTextHint.AntiAliasGridFit;
                g.Clear(GdiColor.Transparent);
                using var font = new GdiFont("Segoe UI Emoji", EMOJI_BITMAP_SIZE * 0.85f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
                using var sf = new GdiStringFormat { Alignment = GdiAlignment.Center, LineAlignment = GdiAlignment.Center };
                using var brush = new GdiBrush(GdiColor.Black);
                g.DrawString(emoji, font, brush, new GdiRectF(0, 0, EMOJI_BITMAP_SIZE, EMOJI_BITMAP_SIZE), sf);
            }
            return ConvertToBitmapSource(bmp);
        }

        private bool HasColor(ImageSource source)
        {
            if (source is not BitmapSource bmp) return false;
            try
            {
                int stride = bmp.PixelWidth * 4;
                byte[] pixels = new byte[bmp.PixelHeight * stride];
                bmp.CopyPixels(pixels, stride, 0);
                for (int y = 0; y < bmp.PixelHeight; y += 4)
                    for (int x = 0; x < bmp.PixelWidth; x += 4)
                    {
                        int idx = y * stride + x * 4;
                        if (pixels[idx + 3] > 20)
                        {
                            int max = Math.Max(pixels[idx + 2], Math.Max(pixels[idx + 1], pixels[idx]));
                            int min = Math.Min(pixels[idx + 2], Math.Min(pixels[idx + 1], pixels[idx]));
                            if (max - min > 25) return true;
                        }
                    }
            }
            catch { }
            return false;
        }

        private ImageSource CreateColorfulFallback(string emoji)
        {
            int hash = 0;
            foreach (char c in emoji) hash = hash * 31 + c;
            double hue = (Math.Abs(hash) % 360) / 360.0;

            using var bmp = new GdiBitmap(EMOJI_BITMAP_SIZE, EMOJI_BITMAP_SIZE, GdiPixelFmt.Format32bppArgb);
            using (var g = GdiGraphics.FromImage(bmp))
            {
                g.SmoothingMode = GdiSmoothing.AntiAlias;
                g.TextRenderingHint = GdiTextHint.AntiAliasGridFit;
                g.Clear(GdiColor.Transparent);
                var rect = new GdiRect(2, 2, EMOJI_BITMAP_SIZE - 4, EMOJI_BITMAP_SIZE - 4);
                using var path = new GdiPath();
                path.AddEllipse(rect);
                using var pgb = new GdiPathBrush(path);
                pgb.CenterColor = HsvToGdiColor(hue, 0.7f, 1.0f);
                pgb.SurroundColors = new[] { HsvToGdiColor((hue + 0.15f) % 1.0, 1.0f, 0.8f) };
                g.FillPath(pgb, path);
                using var font = new GdiFont("Segoe UI Symbol", EMOJI_BITMAP_SIZE * 0.55f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
                using var sf = new GdiStringFormat { Alignment = GdiAlignment.Center, LineAlignment = GdiAlignment.Center };
                using var brush = new GdiBrush(GdiColor.White);
                g.DrawString(emoji, font, brush, new GdiRectF(0, 0, EMOJI_BITMAP_SIZE, EMOJI_BITMAP_SIZE), sf);
            }
            return ConvertToBitmapSource(bmp)!;
        }

        private static ImageSource? ConvertToBitmapSource(GdiBitmap bmp)
        {
            IntPtr hBitmap = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
            finally { DeleteObject(hBitmap); }
        }

        private static GdiColor HsvToGdiColor(double h, float s, float v)
        {
            double r, g, b;
            if (s == 0) { r = g = b = v; }
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
            return GdiColor.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;
            foreach (var img in _emojiPool) _canvas.Children.Add(img);
        }

        public void Draw(List<Point> points)
        {
            if (_canvas == null) return;
            ClearElements();

            switch (_config.Type.ToLowerInvariant())
            {
                case "gradient": DrawGradient(points); break;
                case "pattern": DrawPattern(points); break;
                case "emoji": DrawEmoji(points); break;
                default: DrawGradient(points); break;
            }
        }

        private void DrawGradient(List<Point> points)
        {
            if (_canvas == null || _config.Colors == null || _config.Colors.Count == 0) return;
            var colors = _config.Colors.Select(c => ParseColor(c)).Where(c => c.HasValue).Select(c => c!.Value).ToList();
            if (colors.Count == 0) return;

            var polyline = new Polyline
            {
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeThickness = _config.Thickness,
                IsHitTestVisible = false,
                Points = new PointCollection(points)
            };

            System.Windows.Media.Brush brush;
            if (colors.Count == 1)
                brush = new SolidColorBrush(colors[0]);
            else
            {
                var gb = new LinearGradientBrush();
                for (int i = 0; i < colors.Count; i++)
                    gb.GradientStops.Add(new GradientStop(colors[i], (double)i / (colors.Count - 1)));
                brush = gb;
            }
            polyline.Stroke = brush;

            if (_config.Glow)
                polyline.Effect = new DropShadowEffect { Color = colors[0], BlurRadius = 10, ShadowDepth = 0, Opacity = 0.8 };

            _elements.Add(polyline);
            _canvas.Children.Add(polyline);
        }

        private void DrawPattern(List<Point> points)
        {
            if (_canvas == null || _config.Colors == null || _config.Colors.Count == 0) return;
            var colors = _config.Colors.Select(c => ParseColor(c)).Where(c => c.HasValue).Select(c => c!.Value).ToList();
            if (colors.Count == 0) return;

            int segLen = Math.Max(10, points.Count / colors.Count);
            for (int ci = 0; ci < colors.Count; ci++)
            {
                int start = ci * segLen;
                int end = Math.Min(start + segLen + 1, points.Count);
                if (start >= points.Count) break;

                var segPoints = new PointCollection();
                for (int i = start; i < end; i++) segPoints.Add(points[i]);
                if (segPoints.Count < 2) continue;

                var pl = new Polyline
                {
                    Points = segPoints,
                    Stroke = new SolidColorBrush(colors[ci]),
                    StrokeThickness = _config.Thickness,
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    IsHitTestVisible = false
                };
                if (_config.Glow)
                    pl.Effect = new DropShadowEffect { Color = colors[ci], BlurRadius = 10, ShadowDepth = 0, Opacity = 0.8 };

                _elements.Add(pl);
                _canvas.Children.Add(pl);
            }
        }

        /// <summary>
        /// 🎯 Рисует эмодзи БЕЗ МИГАНИЯ —增量ная отрисовка + детерминированный выбор
        /// </summary>
        private void DrawEmoji(List<Point> points)
        {
            if (_canvas == null || _config.Emojis == null || _config.Emojis.Length == 0) return;
            if (points.Count < 2) return;

            int emojiIndex = 0;
            double accumulatedDistance = 0;

            for (int i = 1; i < points.Count && emojiIndex < MAX_CUSTOM_EMOJIS; i++)
            {
                var p1 = points[i - 1];
                var p2 = points[i];
                double dx = p2.X - p1.X;
                double dy = p2.Y - p1.Y;
                double segLen = Math.Sqrt(dx * dx + dy * dy);
                if (segLen <= 0) continue;

                accumulatedDistance += segLen;

                while (accumulatedDistance >= EMOJI_SPACING_PX && emojiIndex < MAX_CUSTOM_EMOJIS)
                {
                    double t = (accumulatedDistance - EMOJI_SPACING_PX) / segLen;
                    t = Math.Max(0, Math.Min(1, 1 - t));
                    double ex = p1.X + dx * (1 - t);
                    double ey = p1.Y + dy * (1 - t);

                    var img = _emojiPool[emojiIndex];

                    // 🎯 Если эмодзи ещё не назначено — назначаем детерминированно
                    if (string.IsNullOrEmpty(_assignedEmojis[emojiIndex]))
                    {
                        var seededRnd = new Random(emojiIndex * 31 + 17);
                        string emojiText = _config.Emojis[seededRnd.Next(_config.Emojis.Length)];
                        double size = BASE_EMOJI_SIZE + seededRnd.Next(10);
                        double rotation = seededRnd.Next(-25, 25);

                        _assignedEmojis[emojiIndex] = emojiText;
                        _assignedSizes[emojiIndex] = size;
                        _assignedRotations[emojiIndex] = rotation;

                        if (_emojiBitmapCache.TryGetValue(emojiText, out var bitmap))
                            img.Source = bitmap;

                        img.Width = size;
                        img.Height = size;
                        img.RenderTransform = new RotateTransform(rotation);
                    }

                    // Обновляем только позицию
                    double size2 = _assignedSizes[emojiIndex];
                    Canvas.SetLeft(img, ex - size2 / 2);
                    Canvas.SetTop(img, ey - size2 / 2);

                    if (img.Visibility != Visibility.Visible)
                        img.Visibility = Visibility.Visible;

                    emojiIndex++;
                    accumulatedDistance -= EMOJI_SPACING_PX;
                }
            }

            for (int i = emojiIndex; i < _activeEmojis; i++)
            {
                _emojiPool[i].Visibility = Visibility.Collapsed;
                _assignedEmojis[i] = "";
            }
            _activeEmojis = emojiIndex;
        }

        private WpfColor? ParseColor(string hex)
        {
            try { return (WpfColor)WpfColorConverter.ConvertFromString(hex); }
            catch { return null; }
        }

        private void ClearElements()
        {
            if (_canvas == null) return;
            foreach (var el in _elements)
            {
                if (el is Line line) _canvas.Children.Remove(line);
                else if (el is Polyline pl) _canvas.Children.Remove(pl);
                else if (el is Ellipse e) _canvas.Children.Remove(e);
            }
            _elements.Clear();

            for (int i = 0; i < _activeEmojis; i++)
            {
                _emojiPool[i].Visibility = Visibility.Collapsed;
                _assignedEmojis[i] = "";
            }
            _activeEmojis = 0;
        }

        public void Clear() => ClearElements();

        public void SetOpacity(double opacity)
        {
            foreach (var el in _elements)
                if (el is UIElement ui) ui.Opacity = opacity;
            for (int i = 0; i < _activeEmojis; i++)
                _emojiPool[i].Opacity = opacity;
        }

        public void Dispose()
        {
            if (_canvas != null)
            {
                ClearElements();
                foreach (var img in _emojiPool) _canvas.Children.Remove(img);
            }
        }
    }

    public class CustomSkinConfig
    {
        public string Name { get; set; } = "Custom";
        public string Icon { get; set; } = "🎨";
        public string Type { get; set; } = "gradient";
        public List<string> Colors { get; set; } = new() { "#FF50B4" };
        public string[] Emojis { get; set; } = new[] { "✨" };
        public double Thickness { get; set; } = 4.0;
        public bool Glow { get; set; } = true;
    }
}