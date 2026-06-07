<div align="center">

# 💜 GestureFlow

### Control your PC with a flick of your wrist

[![Release](https://img.shields.io/badge/release-v0.9.5-A855F7?style=for-the-badge&logo=github)](https://github.com/BestArmor/GestureFlow/releases)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D4?style=for-the-badge&logo=windows)]()
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet)]()
[![License](https://img.shields.io/badge/license-MIT-10B981?style=for-the-badge)]()
[![Latency](https://img.shields.io/badge/latency-<100ms-FF50B4?style=for-the-badge)]()

[Download](https://github.com/BestArmor/GestureFlow/releases/download/v0.9.5/GestureFlow-v0.9.5.zip) · [Documentation](#documentation) · [Report Bug](https://github.com/BestArmor/GestureFlow/issues)

</div>

---

## 🎯 Overview

GestureFlow is a high-performance gesture recognition system for Windows that lets you control your PC with intuitive mouse movements. Draw gestures in the air and watch your commands execute instantly.

**Why GestureFlow?**

- ⚡ **Ultra-fast**: Less than 100ms latency from gesture to action
- 🎨 **Beautiful**: Premium dark UI with glassmorphism effects
- 🖌️ **Customizable**: Create your own trail skins with JSON
- 🌐 **Multi-language**: Full English/Russian support
- 🔥 **Hot-reload**: Changes apply instantly without restart

---

## ✨ Features

<table>
<tr>
<td width="50%">

### 🎯 Smart Recognition
- Accurate gesture detection
- Minimal false positives
- Configurable thresholds
- Real-time visual feedback

</td>
<td width="50%">

### 🎨 Premium UI
- Modern dark theme
- Glassmorphism effects
- Smooth animations
- Professional design

</td>
</tr>
<tr>
<td width="50%">

### 🖌️ Custom Skins
- 6 built-in styles
- JSON-based configuration
- Hot-reload support
- Unlimited custom skins

</td>
<td width="50%">

### ⚙️ Configurable
- Fine-tune every parameter
- JSON configuration file
- Custom gesture bindings
- Per-app profiles (coming soon)

</td>
</tr>
</table>

---

## 🚀 Quick Start

### Installation

1. **Download** the latest release from [Releases](https://github.com/BestArmor/GestureFlow/releases)
2. **Extract** GestureFlow-v0.9.5.zip to any folder
3. **Run** GestureFlow.exe
4. **Right-click** the system tray icon to access settings

> [!NOTE]
> No installation required. GestureFlow is a portable application.

### Requirements

- Windows 10 / 11 (64-bit)
- .NET 10 Runtime (included in release)
- No admin privileges needed

---

## 🎮 Usage

### Supported Gestures

| Gesture | Action | Default Hotkey |
|:--------|:-------|:---------------|
| ← Swipe Left | Browser Back | Alt + Left |
| → Swipe Right | Browser Forward | Alt + Right |
| ↑ Swipe Up | New Tab | Ctrl + T |
| ↓ Swipe Down | Close Tab | Ctrl + W |
| ○ Circle | Refresh Page | F5 |
| ✓ Checkmark | Minimize Window | Win + D |

### How to Use

1. **Hold** the right mouse button
2. **Draw** a gesture (the trail will appear)
3. **Release** to trigger the action

> [!TIP]
> You can customize all gestures and hotkeys in settings.json.

---

## 🛠️ Tech Stack

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=csharp&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-UI-0078D4?style=for-the-badge&logo=windows)
![WebView2](https://img.shields.io/badge/WebView2-HTML%2FCSS%2FJS-0078D6?style=for-the-badge&logo=microsoftedge)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=for-the-badge&logo=windows)

</div>

---

## 📁 Project Structure

    GestureFlow/
    ├── Core/
    │   ├── GestureRecognizer.cs    # Gesture detection logic
    │   ├── ActionExecutor.cs       # Action mapping
    │   └── MouseHook.cs            # Low-level mouse hooks
    ├── UI/
    │   ├── MainWindow.xaml         # WPF WebView2 host
    │   ├── MainWindow.xaml.cs      # C# <-> JS bridge
    │   └── web/                    # HTML/CSS/JS interface
    │       ├── index.html
    │       ├── styles.css
    │       └── app.js
    ├── Services/
    │   ├── ConfigService.cs        # JSON configuration
    │   ├── SkinManager.cs          # Trail skin management
    │   └── TrayIconManager.cs      # System tray integration
    ├── Models/                     # Data models
    └── settings.json               # User configuration

---

## ⚙️ Configuration

All settings are stored in settings.json. Example:

    {
      "trail": {
        "skin": "Neon",
        "thickness": 4.0,
        "glow": true
      },
      "thresholds": {
        "minGestureLengthPx": 50,
        "moveThresholdPx": 10,
        "holdThresholdMs": 150
      },
      "bindings": {
        "left": "Alt+Left",
        "right": "Alt+Right",
        "up": "Ctrl+T",
        "down": "Ctrl+W",
        "circle": "F5",
        "checkmark": "Win+D"
      }
    }

### Creating Custom Skins

Create a JSON file in the skins/ folder:

    {
      "name": "My Custom Skin",
      "type": "gradient",
      "colors": ["#FF50B4", "#A855F7", "#6366F1"],
      "thickness": 4.0,
      "glow": true
    }

---

## 🗺️ Roadmap

- [x] Core gesture recognition
- [x] 6 built-in trail skins
- [x] Custom skins via JSON
- [x] Hot-reload support
- [x] Premium UI redesign
- [x] Multi-language support (EN/RU)
- [ ] Gesture recording & playback
- [ ] Statistics & analytics dashboard
- [ ] Plugin system for custom actions
- [ ] Cloud sync for settings
- [ ] Per-application gesture profiles
- [ ] Voice command integration

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

### 🐛 Report Bugs
Found a bug? [Open an issue](https://github.com/BestArmor/GestureFlow/issues/new)

### 💡 Suggest Features
Have an idea? [Request a feature](https://github.com/BestArmor/GestureFlow/issues/new)

### 🔧 Submit Pull Requests
1. Fork the repository
2. Create your feature branch (git checkout -b feature/AmazingFeature)
3. Commit your changes (git commit -m 'Add some AmazingFeature')
4. Push to the branch (git push origin feature/AmazingFeature)
5. Open a Pull Request

### 🌟 Star the Repo
If you find this project useful, consider giving it a star!

---

## 📖 Documentation

### Building from Source

    git clone https://github.com/BestArmor/GestureFlow.git
    cd GestureFlow
    dotnet restore
    dotnet build
    dotnet run

### Publishing a Release

    dotnet publish -c Release -o ./publish
    Compress-Archive -Path ./publish/* -DestinationPath GestureFlow-v0.9.5.zip

---

## 📜 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Inspired by modern gesture control systems
- UI design inspired by [Linear](https://linear.app) and [Vercel](https://vercel.com)
- Built with [.NET 10](https://dotnet.microsoft.com/) and [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- WebView2 integration for modern HTML/CSS/JS interface

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/BestArmor/GestureFlow/issues)
- **Discussions**: [GitHub Discussions](https://github.com/BestArmor/GestureFlow/discussions)

---

<div align="center">

### Made with 💜 by [BestArmor](https://github.com/BestArmor)

**If GestureFlow helps you, consider giving it a ⭐!**

[![Stars](https://img.shields.io/github/stars/BestArmor/GestureFlow?style=social)](https://github.com/BestArmor/GestureFlow/stargazers)
[![Forks](https://img.shields.io/github/forks/BestArmor/GestureFlow?style=social)](https://github.com/BestArmor/GestureFlow/network/members)
[![Watchers](https://img.shields.io/github/watchers/BestArmor/GestureFlow?style=social)](https://github.com/BestArmor/GestureFlow/watchers)

</div>
