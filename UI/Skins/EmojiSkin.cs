using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// 🔑 WPF Aliases
using Point = System.Windows.Point;
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
using GdiInterpolation = System.Drawing.Drawing2D.InterpolationMode;
using GdiPixelFmt = System.Drawing.Imaging.PixelFormat;

namespace GestureFlow.UI.Skins
{
    public class EmojiSkin : ITrailSkin
    {
        private const int MAX_EMOJIS = 100;
        private const double EMOJI_SPACING_PX = 50.0;
        private const int EMOJI_BITMAP_SIZE = 64;
        private const double BASE_EMOJI_SIZE = 32.0;

        private static readonly string[] _emojiSet = {
            "😊", "😢", "😄", "😭", "😎", "😡", "😱", "🤩", "🥳", "😈",
            "👻", "💀", "👽", "🤖", "🎃", "🎄", "🎁", "🎈", "🎉", "✨",
            "⭐", "🌟", "💫", "💥", "🔥", "💜", "💙", "💚", "💛", "🧡",
            "❤️", "🖤", "🤍", "⚡", "💨", "🌈", "☀️", "🌙", "🦄", "🐱",
            "🐶", "🌸", "🌺", "🍀", "🎵", "🎶", "💎", "👑", "🏆", "🎯"
        };

        private static readonly Dictionary<string, ImageSource> _bitmapCache = new();
        private static bool _cacheBuilt = false;
        private static readonly object _cacheLock = new();

        private Canvas? _canvas;
        private readonly WpfImage[] _emojiPool = new WpfImage[MAX_EMOJIS];
        private readonly string[] _assignedEmojis = new string[MAX_EMOJIS]; // 🎯 Запоминаем какое эмодзи назначено
        private readonly double[] _assignedSizes = new double[MAX_EMOJIS]; // 🎯 Запоминаем размер
        private readonly double[] _assignedRotations = new double[MAX_EMOJIS]; // 🎯 Запоминаем поворот
        private readonly Random _rnd = new();
        private int _activeEmojis = 0;

        public string Name => "Emoji";
        public string Icon => "🎉";

        public EmojiSkin()
        {
            BuildCache();

            for (int i = 0; i < MAX_EMOJIS; i++)
            {
                _emojiPool[i] = new WpfImage
                {
                    Width = BASE_EMOJI_SIZE,
                    Height = BASE_EMOJI_SIZE,
                    Stretch = Stretch.Uniform,
                    IsHitTestVisible = false,
                    Visibility = Visibility.Collapsed,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                _assignedEmojis[i] = "";
                _assignedSizes[i] = 0;
                _assignedRotations[i] = 0;
            }
        }

        private static void BuildCache()
        {
            lock (_cacheLock)
            {
                if (_cacheBuilt) return;

                Console.WriteLine("🎨 Строим кэш цветных эмодзи...");
                int colorCount = 0, fallbackCount = 0;

                foreach (var emoji in _emojiSet)
                {
                    if (_bitmapCache.ContainsKey(emoji)) continue;
                    try
                    {
                        var source = TryRenderColorEmoji(emoji);
                        if (source != null && HasColor(source))
                        {
                            _bitmapCache[emoji] = source;
                            colorCount++;
                        }
                        else
                        {
                            _bitmapCache[emoji] = CreateColorfulFallback(emoji);
                            fallbackCount++;
                        }
                    }
                    catch
                    {
                        _bitmapCache[emoji] = CreateColorfulFallback(emoji);
                        fallbackCount++;
                    }
                }

                _cacheBuilt = true;
                Console.WriteLine($"✅ Кэш готов: {colorCount} цветных, {fallbackCount} фоллбэк");
            }
        }

        private static ImageSource? TryRenderColorEmoji(string emoji)
        {
            using var bmp = new GdiBitmap(EMOJI_BITMAP_SIZE, EMOJI_BITMAP_SIZE, GdiPixelFmt.Format32bppArgb);
            using (var g = GdiGraphics.FromImage(bmp))
            {
                g.SmoothingMode = GdiSmoothing.AntiAlias;
                g.TextRenderingHint = GdiTextHint.AntiAliasGridFit;
                g.InterpolationMode = GdiInterpolation.HighQualityBicubic;
                g.Clear(GdiColor.Transparent);

                using var font = new GdiFont("Segoe UI Emoji", EMOJI_BITMAP_SIZE * 0.85f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
                using var sf = new GdiStringFormat { Alignment = GdiAlignment.Center, LineAlignment = GdiAlignment.Center };
                using var brush = new GdiBrush(GdiColor.Black);
                g.DrawString(emoji, font, brush, new GdiRectF(0, 0, EMOJI_BITMAP_SIZE, EMOJI_BITMAP_SIZE), sf);
            }
            return ConvertToBitmapSource(bmp);
        }

        private static bool HasColor(ImageSource source)
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

        private static ImageSource CreateColorfulFallback(string emoji)
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

        /// <summary>
        /// 🎯 КЛЮЧЕВОЙ МЕТОД:增量ная отрисовка без мигания.
        /// - Для существующих эмодзи: только обновляем позицию (не меняем сам эмодзи!)
        /// - Для новых эмодзи: назначаем детерминированно по индексу
        /// </summary>
        public void Draw(List<Point> points)
        {
            if (_canvas == null || points.Count < 2) return;

            int emojiIndex = 0;
            double accumulatedDistance = 0;

            for (int i = 1; i < points.Count && emojiIndex < MAX_EMOJIS; i++)
            {
                var p1 = points[i - 1];
                var p2 = points[i];
                double dx = p2.X - p1.X;
                double dy = p2.Y - p1.Y;
                double segLen = Math.Sqrt(dx * dx + dy * dy);
                if (segLen <= 0) continue;

                accumulatedDistance += segLen;

                while (accumulatedDistance >= EMOJI_SPACING_PX && emojiIndex < MAX_EMOJIS)
                {
                    double t = (accumulatedDistance - EMOJI_SPACING_PX) / segLen;
                    t = Math.Max(0, Math.Min(1, 1 - t));
                    double ex = p1.X + dx * (1 - t);
                    double ey = p1.Y + dy * (1 - t);

                    var emoji = _emojiPool[emojiIndex];

                    // 🎯 Если эмодзи ещё не назначено на эту позицию — назначаем ДЕТЕРМИНИРОВАННО
                    if (string.IsNullOrEmpty(_assignedEmojis[emojiIndex]))
                    {
                        // Seed на основе индекса = всегда одно и то же эмодзи на позиции N
                        var seededRnd = new Random(emojiIndex * 31 + 17);
                        string emojiText = _emojiSet[seededRnd.Next(_emojiSet.Length)];
                        double size = BASE_EMOJI_SIZE + seededRnd.Next(10);
                        double rotation = seededRnd.Next(-25, 25);

                        _assignedEmojis[emojiIndex] = emojiText;
                        _assignedSizes[emojiIndex] = size;
                        _assignedRotations[emojiIndex] = rotation;

                        if (_bitmapCache.TryGetValue(emojiText, out var bitmap))
                            emoji.Source = bitmap;

                        emoji.Width = size;
                        emoji.Height = size;
                        emoji.RenderTransform = new RotateTransform(rotation);
                    }

                    // Обновляем только ПОЗИЦИЮ (эмодзи, размер, поворот НЕ меняем!)
                    double size2 = _assignedSizes[emojiIndex];
                    Canvas.SetLeft(emoji, ex - size2 / 2);
                    Canvas.SetTop(emoji, ey - size2 / 2);

                    if (emoji.Visibility != Visibility.Visible)
                        emoji.Visibility = Visibility.Visible;

                    emojiIndex++;
                    accumulatedDistance -= EMOJI_SPACING_PX;
                }
            }

            // Скрываем эмодзи которые больше не нужны (путь стал короче)
            for (int i = emojiIndex; i < _activeEmojis; i++)
            {
                _emojiPool[i].Visibility = Visibility.Collapsed;
                _assignedEmojis[i] = ""; // Сбрасываем для следующего жеста
            }

            _activeEmojis = emojiIndex;
        }

        public void Clear()
        {
            for (int i = 0; i < _activeEmojis; i++)
            {
                _emojiPool[i].Visibility = Visibility.Collapsed;
                _assignedEmojis[i] = ""; // 🎯 Сбрасываем назначения
            }
            _activeEmojis = 0;
        }

        public void SetOpacity(double opacity)
        {
            for (int i = 0; i < _activeEmojis; i++)
                _emojiPool[i].Opacity = opacity;
        }

        public void Dispose()
        {
            if (_canvas != null)
                foreach (var img in _emojiPool) _canvas.Children.Remove(img);
        }
    }
}