using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GestureFlow.Models;
using GestureFlow.UI.Skins;

namespace GestureFlow.Services
{
    /// <summary>
    /// 🎨 Менеджер скинов — загружает встроенные и пользовательские скины.
    /// </summary>
    public class SkinManager
    {
        private readonly string _skinsDirectory;
        private readonly Dictionary<string, ITrailSkin> _skins = new();

        public SkinManager()
        {
            _skinsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skins");
            LoadBuiltinSkins();
            LoadCustomSkins();
        }

        /// <summary>
        /// Все доступные скины
        /// </summary>
        public IReadOnlyDictionary<string, ITrailSkin> Skins => _skins;

        /// <summary>
        /// Получить скин по имени (с fallback на Solid)
        /// </summary>
        public ITrailSkin GetSkin(string name, TrailSettings trailSettings)
        {
            if (_skins.TryGetValue(name, out var skin))
                return skin;

            // Если не нашли — создаём новый Solid с текущими настройками
            Console.WriteLine($"⚠️ Скин '{name}' не найден, используем Solid");
            return new SolidSkin(trailSettings.Color, trailSettings.Thickness);
        }

        /// <summary>
        /// Список имён всех скинов (для UI)
        /// </summary>
        public IEnumerable<string> GetAvailableSkinNames() => _skins.Keys;

        /// <summary>
        /// Перезагружает пользовательские скины (hot-reload)
        /// </summary>
        public void ReloadCustomSkins()
        {
            // Удаляем только custom скины (встроенные не трогаем)
            var customKeys = _skins.Keys.Where(k => !_builtinSkinNames.Contains(k)).ToList();
            foreach (var key in customKeys)
                _skins.Remove(key);

            LoadCustomSkins();
        }

        private void LoadBuiltinSkins()
        {
            // Встроенные скины создаются "по запросу" с нужными параметрами
            // Здесь регистрируем их имена как доступные
            foreach (var name in _builtinSkinNames)
            {
                _skins[name] = CreateBuiltinSkin(name);
            }
        }

        private void LoadCustomSkins()
        {
            try
            {
                if (!Directory.Exists(_skinsDirectory))
                {
                    Directory.CreateDirectory(_skinsDirectory);
                    CreateSampleSkin();
                    Console.WriteLine($"📁 Создана папка skins/ с примером");
                }

                var jsonFiles = Directory.GetFiles(_skinsDirectory, "*.json");
                foreach (var file in jsonFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var config = JsonSerializer.Deserialize<CustomSkinConfig>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (config != null)
                        {
                            var skin = new CustomSkin(config);
                            _skins[config.Name] = skin;
                            Console.WriteLine($"✅ Загружен custom скин: {config.Icon} {config.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Ошибка загрузки {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки custom скинов: {ex.Message}");
            }
        }

        private void CreateSampleSkin()
        {
            var sample = new CustomSkinConfig
            {
                Name = "Sunset",
                Icon = "🌅",
                Type = "gradient",
                Colors = new List<string> { "#FF6B6B", "#FFE66D", "#FF8E53" },
                Thickness = 5.0,
                Glow = true
            };

            var json = JsonSerializer.Serialize(sample, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(_skinsDirectory, "sunset.json"), json);
        }

        private ITrailSkin CreateBuiltinSkin(string name)
        {
            // Возвращаем "пустышку" с правильным именем
            // Реальные параметры подставятся при фактическом создании скина
            return name switch
            {
                "Solid" => new SolidSkin(),
                "Rainbow" => new RainbowSkin(),
                "Blink" => new BlinkSkin(),
                "Particle" => new ParticleSkin(),
                "Emoji" => new EmojiSkin(),
                _ => new SolidSkin()
            };
        }

        private readonly HashSet<string> _builtinSkinNames = new()
        {
            "Solid", "Rainbow", "Blink", "Particle", "Emoji"
        };

        /// <summary>
        /// Создаёт инстанс скина с правильными параметрами (цвет, толщина)
        /// </summary>
        public ITrailSkin CreateSkinInstance(string name, TrailSettings settings)
        {
            // Для встроенных скинов — создаём новый инстанс с параметрами
            return name switch
            {
                "Solid" => new SolidSkin(settings.Color, settings.Thickness),
                "Rainbow" => new RainbowSkin(settings.Thickness),
                "Blink" => new BlinkSkin(settings.Color, settings.Thickness),
                "Particle" => new ParticleSkin(settings.Color, settings.Thickness),
                "Emoji" => new EmojiSkin(),
                _ when _skins.TryGetValue(name, out var customSkin) => customSkin,
                _ => new SolidSkin(settings.Color, settings.Thickness)
            };
        }
    }
}