using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GestureFlow.UI.Skins
{
    /// <summary>
    /// 🎨 Интерфейс для скинов трейла.
    /// Каждый скин реализует свою логику отрисовки.
    /// </summary>
    public interface ITrailSkin
    {
        /// <summary>
        /// Название скина для отображения в UI
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Иконка/эмодзи для отображения в UI
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// Инициализирует скин (создаёт необходимые UI элементы)
        /// </summary>
        void Initialize(System.Windows.Controls.Canvas canvas);

        /// <summary>
        /// Отрисовывает трейл по точкам
        /// </summary>
        void Draw(List<System.Windows.Point> points);

        /// <summary>
        /// Очищает трейл
        /// </summary>
        void Clear();

        /// <summary>
        /// Устанавливает прозрачность (для fade-эффекта)
        /// </summary>
        void SetOpacity(double opacity);

        /// <summary>
        /// Освобождает ресурсы
        /// </summary>
        void Dispose();
    }
}