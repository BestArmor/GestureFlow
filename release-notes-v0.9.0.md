<p align="center">
  <img src="https://raw.githubusercontent.com/BestArmor/GestureFlow/main/assets/banner.png" alt="GestureFlow" width="800"/>
</p>

<p align="center">
  <a href="https://github.com/BestArmor/GestureFlow/releases"><img src="https://img.shields.io/badge/version-0.9.0-FF50B4?style=for-the-badge"/></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-FF50B4?style=for-the-badge"/></a>
  <a href="#"><img src="https://img.shields.io/badge/.NET-10-FF50B4?style=for-the-badge"/></a>
  <a href="#"><img src="https://img.shields.io/badge/WPF-Modern_UI-FF50B4?style=for-the-badge"/></a>
</p>

---

## 🎉 v0.9.0 — Modern GUI Application!

GestureFlow получил **полноценный графический интерфейс** в современном стиле!

---

## ✨ Что нового в v0.9.0

| Фича | Описание |
|---|---|
| 🏛️ **Главное окно** | Современный WPF UI с тёмной темой |
| 📑 **4 функциональных таба** | Жесты, Настройки, Статистика, О программе |
| 🎨 **Fluent Design** | Закруглённые углы, hover-эффекты, плавные transitions |
| 💜 **Фиолетовые акценты** | Фирменный стиль #FF50B4 |
| 🖱️ **Двойной клик по иконке** | Открывает главное окно |
| ⚙️ **Кнопка настроек** | Открывает `settings.json` одним кликом |
| 🔗 **Ссылка на GitHub** | Прямой переход в репозиторий |

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

## 🖥️ Скриншоты GUI

### 🎬 Вкладка "Жесты"
Красивые карточки со всеми доступными жестами и их описаниями.

### ⚙️ Вкладка "Настройки"
- Быстрый доступ к `settings.json`
- Информация о hot-reload
- Анонс будущих скинов и профилей

### 📊 Вкладка "Статистика"
Placeholder для будущей системы подсчёта жестов (в v1.0).

### ℹ️ Вкладка "О программе"
- Описание особенностей
- Прямая ссылка на GitHub
- Информация о версии

---

## 📝 Конфигурация

Все настройки по-прежнему в `settings.json` с поддержкой **hot-reload**:

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
💡 Pro tip: Откройте settings.json прямо из GUI одним кликом!
🗺️ Roadmap
✅ v0.5 — Базовые жесты
✅ v0.6 — Сложные жесты (⭕✅)
✅ v0.7 — Конфигурация (settings.json + hot-reload)
✅ v0.8 — Иконка в трее
✅ v0.9 — GUI приложение (← ты здесь)
🔜 v0.9.5 — Скины трейла (Rainbow, Particle, Emoji)
🔜 v0.9.7 — Умные профили (YouTube, Spotify, Discord)
🔜 v1.0 — Макросы + финальная полировка

🚀 Установка:
1. Скачай GestureFlow-v0.9.0.zip ниже
2. Распакуй в любую папку
3. Запусти GestureFlow.exe
4. Ищи фиолетовую иконку "G" в системном трее
5. Двойной клик → открывается красивое главное окно! 💜
🎨 Технические детали:
UI фреймворк: WPF (.NET 10)
Тема: Dark mode (#1E1E2E фон, #2D2D44 карточки)
Акцент: #FF50B4 (neon purple)
Шрифт: Segoe UI
Архитектура: MVVM-ready (готово к расширению)
<p align="center">
<b>Made with 💜 by BestArmor</b><br/>
<sub>Built with .NET 10 and WPF</sub>
</p>