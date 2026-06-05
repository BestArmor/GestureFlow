namespace GestureFlow.Core
{
    public enum GestureType
    {
        None,
        Left,       // ← базовый
        Right,      // → базовый
        Up,         // ↑ базовый
        Down,       // ↓ базовый
        Circle,     // ⭕ обновить страницу (F5)
        Checkmark   // ✅ свернуть окно (Win+Down x2)
    }
}