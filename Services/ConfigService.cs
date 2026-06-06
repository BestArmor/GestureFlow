using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using GestureFlow.Models;

namespace GestureFlow.Services
{
    /// <summary>
    /// ⚙️ Сервис управления настройками приложения.
    /// </summary>
    public class ConfigService : IDisposable
    {
        private const string ConfigFileName = "settings.json";
        private readonly string _configPath;
        private FileSystemWatcher? _watcher;
        
        // 🔑 ВАЖНО: БЕЗ readonly — поле должно перезаписываться в Reload()
        private AppSettings _settings;

        public event Action<AppSettings>? SettingsChanged;

        public ConfigService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            _settings = LoadOrCreate();
            SetupWatcher();
        }

        public AppSettings Current => _settings;

        private AppSettings LoadOrCreate()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, options);
                    if (settings != null)
                    {
                        Console.WriteLine($"✅ Настройки загружены из {ConfigFileName}");
                        return settings;
                    }
                }

                Console.WriteLine($"📝 Создаём дефолтный {ConfigFileName}");
                var defaults = new AppSettings();
                Save(defaults);
                return defaults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки конфига: {ex.Message}");
                Console.WriteLine("📝 Используем дефолтные настройки");
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_configPath, json);
                _settings = settings;
                Console.WriteLine($"💾 Настройки сохранены в {ConfigFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения конфига: {ex.Message}");
            }
        }

        public void Reload()
        {
            // ✅ Теперь _settings не readonly — можно присваивать
            _settings = LoadOrCreate();
            SettingsChanged?.Invoke(_settings);
            Console.WriteLine("🔄 Настройки перезагружены");
        }

        private void SetupWatcher()
        {
            try
            {
                string? directory = Path.GetDirectoryName(_configPath);
                if (string.IsNullOrEmpty(directory)) return;

                // 🔑 Создаём watcher в локальную переменную чтобы избежать CS8602
                var watcher = new FileSystemWatcher(directory, ConfigFileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                watcher.Changed += (s, e) =>
                {
                    System.Threading.Thread.Sleep(100);
                    Console.WriteLine("🔍 Обнаружено изменение settings.json");
                    Reload();
                };

                _watcher = watcher;
                Console.WriteLine("👀 Hot-reload активирован");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Не удалось настроить hot-reload: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}