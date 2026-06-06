<p align="center">
  <img src="https://raw.githubusercontent.com/BestArmor/GestureFlow/main/assets/banner.png" alt="GestureFlow" width="800"/>
</p>

<p align="center">
  <a href="https://github.com/BestArmor/GestureFlow/releases"><img src="https://img.shields.io/badge/version-0.7.0-FF50B4?style=for-the-badge"/></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-FF50B4?style=for-the-badge"/></a>
  <a href="#"><img src="https://img.shields.io/badge/.NET-10-FF50B4?style=for-the-badge"/></a>
</p>

---

## ✨ Что нового в v0.7.0

🎯 **Полная система конфигурации через `settings.json`!**

| Фича | Описание |
|---|---|
| 📄 **settings.json** | Автоматическое создание файла настроек при первом запуске |
| ⚙️ **Настраиваемые пороги** | Точная настройка чувствительности жестов |
| 🎨 **Кастомные цвета трейла** | Любой HEX цвет — от фиолетового до неонового зелёного |
| 📏 **Регулируемая толщина** | Делай трейл тоньше или толще |
| 🔥 **Hot-reload** | Изменения применяются мгновенно без перезапуска! |
| 🛡️ **Защита от ошибок** | Автоматический fallback на дефолты если конфиг сломан |

---

## 🎬 Все жесты

| Жест | Действие |
|:---:|---|
| ← | Назад в браузере (`Alt+←`) |
| → | Вперёд в браузере (`Alt+→`) |
| ↑ | Новая вкладка (`Ctrl+T`) |
| ↓ | Закрыть вкладку (`Ctrl+W`) |
| ⭕ | Обновить страницу (`F5`) |
| ✅ | Свернуть активное окно |

---

## 📝 Пример `settings.json`

```json
{
  "Thresholds": {
    "MoveThresholdPx": 5.0,
    "HoldThresholdMs": 100,
    "MinGestureLengthPx": 30.0
  },
  "Trail": {
    "Color": "#FF50B4",
    "Thickness": 4.0
  }
}

💡 Pro tip: Изменяй файл и смотри как настройки применяются мгновенно!
🗺️ Roadmap
✅ v0.5 — Базовые жесты
✅ v0.6 — Сложные жесты (⭕✅)
✅ v0.7 — Конфигурация (← ты здесь)
🔜 v0.8 — Иконка в трее
🔜 v0.9 — Умные профили (YouTube, Spotify)
🔜 v1.0 — Полноценный GUI
<p align="center"><b>Made with 💜 by BestArmor</b></p>
```