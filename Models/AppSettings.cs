namespace GestureFlow.Models
{
    public class AppSettings
    {
        public ThresholdSettings Thresholds { get; set; } = new();
        public TrailSettings Trail { get; set; } = new();
        public BindingsSettings Bindings { get; set; } = new();
        public GeneralSettings General { get; set; } = new();
    }

    public class ThresholdSettings
    {
        public double MoveThresholdPx { get; set; } = 5.0;
        public int HoldThresholdMs { get; set; } = 100;
        public double MinGestureLengthPx { get; set; } = 30.0;
    }

    public class TrailSettings
    {
        public string Color { get; set; } = "#FF50B4";
        public double Thickness { get; set; } = 4.0;
    }

    public class BindingsSettings
    {
        public GestureBindings Global { get; set; } = new()
        {
            Left = "Alt+Left",
            Right = "Alt+Right",
            Up = "Ctrl+T",
            Down = "Ctrl+W",
            Circle = "F5",
            Checkmark = "MINIMIZE"
        };

        public GestureBindings YouTube { get; set; } = new()
        {
            Left = "Left",
            Right = "Right",
            Up = "Up",
            Down = "Down"
        };
    }

    public class GestureBindings
    {
        public string Left { get; set; } = "";
        public string Right { get; set; } = "";
        public string Up { get; set; } = "";
        public string Down { get; set; } = "";
        public string Circle { get; set; } = "";
        public string Checkmark { get; set; } = "";
    }

    public class GeneralSettings
    {
        public bool StartWithWindows { get; set; } = false;
        public bool StartMinimized { get; set; } = true;
    }
}