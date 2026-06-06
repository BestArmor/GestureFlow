namespace GestureFlow.Models
{
    /// <summary>
    /// 📋 Корневой объект настроек приложения.
    /// Читается из settings.json при запуске.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Пороги распознавания жестов
        /// </summary>
        public ThresholdSettings Thresholds { get; set; } = new();

        /// <summary>
        /// Настройки визуального трейла
        /// </summary>
        public TrailSettings Trail { get; set; } = new();

        /// <summary>
        /// Привязки жестов к действиям
        /// </summary>
        public BindingsSettings Bindings { get; set; } = new();

        /// <summary>
        /// Общие настройки приложения
        /// </summary>
        public GeneralSettings General { get; set; } = new();
    }

    /// <summary>
    /// ⏱ Пороги распознавания жестов
    /// </summary>
    public class ThresholdSettings
    {
        /// <summary>
        /// Минимальное движение в пикселях для активации режима жеста
        /// </summary>
        public double MoveThresholdPx { get; set; } = 5.0;

        /// <summary>
        /// Минимальное время удержания ПКМ в миллисекундах
        /// </summary>
        public int HoldThresholdMs { get; set; } = 100;

        /// <summary>
        /// Минимальная длина жеста для распознавания
        /// </summary>
        public double MinGestureLengthPx { get; set; } = 30.0;
    }

    /// <summary>
    /// 🎨 Настройки трейла (цвет, толщина, скин)
    /// </summary>
    public class TrailSettings
    {
        /// <summary>
        /// 🆕 Название скина: Solid, Rainbow, Blink, Particle, Emoji или имя custom-скина
        /// </summary>
        public string Skin { get; set; } = "Solid";

        /// <summary>
        /// Цвет трейла в HEX формате (используется в Solid, Blink, Particle)
        /// </summary>
        public string Color { get; set; } = "#FF50B4";

        /// <summary>
        /// Толщина линии трейла в пикселях
        /// </summary>
        public double Thickness { get; set; } = 4.0;
    }

    /// <summary>
    /// 🎯 Привязки жестов к действиям (глобальные и per-app)
    /// </summary>
    public class BindingsSettings
    {
        /// <summary>
        /// Глобальные привязки (работают везде)
        /// </summary>
        public GestureBindings Global { get; set; } = new()
        {
            Left = "Alt+Left",
            Right = "Alt+Right",
            Up = "Ctrl+T",
            Down = "Ctrl+W",
            Circle = "F5",
            Checkmark = "MINIMIZE"
        };

        /// <summary>
        /// Привязки для YouTube (в браузерах с открытым видео)
        /// </summary>
        public GestureBindings YouTube { get; set; } = new()
        {
            Left = "Left",
            Right = "Right",
            Up = "Up",
            Down = "Down"
        };
    }

    /// <summary>
    /// 🎬 Привязки для одного набора жестов
    /// </summary>
    public class GestureBindings
    {
        public string Left { get; set; } = "";
        public string Right { get; set; } = "";
        public string Up { get; set; } = "";
        public string Down { get; set; } = "";
        public string Circle { get; set; } = "";
        public string Checkmark { get; set; } = "";
    }

    /// <summary>
    /// ⚙️ Общие настройки приложения
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Запускать вместе с Windows
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// Запускать свёрнутым в трей
        /// </summary>
        public bool StartMinimized { get; set; } = true;
    }
}