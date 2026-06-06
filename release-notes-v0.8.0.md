<p align="center">
  <img src="https://raw.githubusercontent.com/BestArmor/GestureFlow/main/assets/banner.png" alt="GestureFlow" width="800"/>
</p>

<p align="center">
  <a href="https://github.com/BestArmor/GestureFlow/releases"><img src="https://img.shields.io/badge/version-0.8.0-FF50B4?style=for-the-badge"/></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-FF50B4?style=for-the-badge"/></a>
  <a href="#"><img src="https://img.shields.io/badge/.NET-10-FF50B4?style=for-the-badge"/></a>
</p>

---

## 🎉 v0.8.0 — Desktop Application!

GestureFlow теперь **полноценное десктопное приложение** с иконкой в системном трее!

---

## ✨ Что нового в v0.8.0

| Фича | Описание |
|---|---|
| 🎯 **Иконка в трее** | Фиолетовая круглая иконка с буквой "G" возле часов Windows |
| 📋 **Контекстное меню** | ПКМ по иконке → "О программе", "Открыть settings.json", "Выход" |
| 🚪 **Нормальное закрытие** | Больше не нужно искать терминал чтобы закрыть приложение |
| 💡 **Toast-уведомления** | Ненавязчивое уведомление при запуске |
| ⚙️ **StartMinimized** | Использует настройку из `settings.json` |

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

## 🎯 Как использовать

1. **Запусти** `GestureFlow.exe`
2. **Ищи иконку** в системном трее (возле часов Windows)
3. **ПКМ по иконке** → контекстное меню:
   - 🎨 **GestureFlow v0.8** — окно "О программе"
   - ⚙️ **Открыть settings.json** — откроет конфиг в дефолтном редакторе
   - 🚪 **Выход** — корректно закроет приложение
4. **Двойной клик** по иконке — тоже откроет "О программе"

---

## 📝 Конфигурация

Все настройки в `settings.json`:

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
  },
  "General": {
    "StartWithWindows": false,
    "StartMinimized": true
  }
}
```
💡 Pro tip: Изменяй settings.json и смотри как настройки применяются мгновенно (hot-reload)!
🗺️ Roadmap
✅ v0.5 — Базовые жесты (←→↑↓)
✅ v0.6 — Сложные жесты (⭕✅)
✅ v0.7 — Конфигурация (settings.json + hot-reload)
✅ v0.8 — Иконка в трее (← ты здесь)
🔜 v0.9 — Умные профили (YouTube, Spotify, Discord)
🔜 v0.9.5 — Скины трейла (Rainbow, Particle, Emoji)
🔜 v0.9.9 — Полноценный GUI
🔜 v1.0 — Макросы + финальная полировка

🚀 Установка:
1. Скачай GestureFlow-v0.8.0.zip ниже
2. Распакуй в любую папку
3. Запусти GestureFlow.exe
4. Ищи фиолетовую иконку "G" в системном трее 💜
<p align="center">
<b>Made with 💜 by BestArmor</b><br/>
<sub>Built with .NET 10 & WPF</sub>
</p>