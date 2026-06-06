using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestureFlow.Services
{
    /// <summary>
    /// 🎯 Менеджер иконки в системном трее.
    /// </summary>
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;

        public event Action? ExitRequested;
        public event Action? AboutRequested;
        public event Action? OpenSettingsRequested;

        public TrayIconManager()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("🎨 GestureFlow v0.8", null, (s, e) => AboutRequested?.Invoke());
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("⚙️ Открыть settings.json", null, (s, e) => OpenSettingsRequested?.Invoke());
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("🚪 Выход", null, (s, e) => ExitRequested?.Invoke());

            _notifyIcon = new NotifyIcon
            {
                Icon = CreateDefaultIcon(),
                Text = "GestureFlow — управление жестами",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            _notifyIcon.DoubleClick += (s, e) => AboutRequested?.Invoke();
        }

        private Icon CreateDefaultIcon()
        {
            var bitmap = new Bitmap(32, 32);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Фиолетовый фон с закруглением (имитация)
            graphics.Clear(Color.Transparent);
            using (var brush = new SolidBrush(Color.FromArgb(255, 80, 180)))
            {
                graphics.FillEllipse(brush, 0, 0, 31, 31);
            }
            
            // Белая буква G
            using var font = new Font("Segoe UI", 18, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            var textSize = graphics.MeasureString("G", font);
            float x = (32 - textSize.Width) / 2;
            float y = (32 - textSize.Height) / 2;
            graphics.DrawString("G", font, textBrush, x, y);

            return Icon.FromHandle(bitmap.GetHicon());
        }

        public void ShowNotification(string title, string text)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = text;
            _notifyIcon.ShowBalloonTip(3000);
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
    }
}