using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;
using GestureFlow.Services;
using GestureFlow.Models;

namespace GestureFlow.UI
{
    public partial class MainWindow : Window
    {
        private readonly ConfigService? _configService;
        private readonly SkinManager? _skinManager;

        public MainWindow() { InitializeComponent(); }

        public MainWindow(ConfigService? configService, SkinManager? skinManager)
        {
            InitializeComponent();
            _configService = configService;
            _skinManager = skinManager;
            LoadSkins();
        }

        private void LoadSkins()
        {
            if (_skinManager == null || SkinComboBox == null) return;
            SkinComboBox.Items.Clear();
            foreach (var skin in _skinManager.Skins.Values)
                SkinComboBox.Items.Add(new SkinItem { Name = skin.Name, DisplayName = $"{skin.Icon} {skin.Name}" });
            if (_configService != null)
            {
                string current = _configService.Current.Trail.Skin;
                for (int i = 0; i < SkinComboBox.Items.Count; i++)
                    if (SkinComboBox.Items[i] is SkinItem item && item.Name == current)
                    { SkinComboBox.SelectedIndex = i; break; }
            }
        }

        private void SkinComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SkinComboBox.SelectedItem is SkinItem selected && _configService != null)
            {
                _configService.Current.Trail.Skin = selected.Name;
                _configService.Save(_configService.Current);
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            try {
                string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(p)) Process.Start(new ProcessStartInfo { FileName = p, UseShellExecute = true });
            } catch {}
        }

        private void OpenSkinsFolder_Click(object sender, RoutedEventArgs e)
        {
            try {
                string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skins");
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                Process.Start(new ProcessStartInfo { FileName = p, UseShellExecute = true });
            } catch {}
        }

        private void OpenGitHub_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo { FileName = "https://github.com/BestArmor/GestureFlow", UseShellExecute = true }); } catch {}
        }

        private class SkinItem
        {
            public string Name { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public override string ToString() => DisplayName;
        }
    }
}
