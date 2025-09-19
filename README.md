# Terraria Tweaker - Game Hack Menu

A modern, feature-rich hack menu for Terraria built with C# and ImGui.NET. This educational tool demonstrates memory manipulation techniques and provides quality-of-life improvements for single-player Terraria sessions.

![Terraria Tweaker Logo](https://via.placeholder.com/400x200/1a1a2e/16537e?text=Terraria+Tweaker)

## ⚠️ Important Disclaimer

**This tool is for educational purposes and single-player use only!**

- Only use this for learning about memory manipulation and reverse engineering
- Never use in multiplayer or online modes
- Always backup your save files before using any cheats
- Memory addresses change between game updates - you'll need to update them
- Use at your own risk - the authors are not responsible for any damage

## ✨ Features

### Player Modifications
- **Health Management**: Set, freeze, or refill health instantly
- **Mana Control**: Manipulate mana values with precision
- **Position Teleportation**: Move anywhere on the map instantly
- **Position Freezing**: Lock your character in place

### Inventory & Economy
- **Coin Manipulation**: Set any amount of copper, silver, gold, or platinum coins
- **Quick Money**: Instantly give yourself maximum currency
- **Item Management**: Framework for future item manipulation features

### World Control
- **Time Manipulation**: Set specific times of day or freeze time
- **Weather Control**: Framework for weather modifications
- **World State**: Control various world parameters

### Quality of Life
- **Modern UI**: Clean, dark theme with tabbed interface
- **Real-time Values**: Live display of current game values
- **Hotkeys**: Configurable keyboard shortcuts (framework included)
- **Configuration**: Persistent settings and preferences
- **Logging**: Comprehensive logging system for debugging

## 🛠️ Setup Instructions

### Prerequisites

1. **.NET 8.0 SDK** - Download from [Microsoft](https://dotnet.microsoft.com/download)
2. **Cheat Engine** - Download latest version from [cheatengine.org](https://cheatengine.org/)
3. **Visual Studio 2022** (optional) - For development
4. **Administrator privileges** - Required for memory manipulation

### Installation

1. **Clone or download** this project to your computer
2. **Open Command Prompt as Administrator** and navigate to the project folder
3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```
4. **Build the project**:
   ```bash
   dotnet build --configuration Release
   ```

## 🎯 Finding Memory Addresses with Cheat Engine

**This is the most important part!** The placeholder addresses in the code won't work until you find the real ones.

### Step-by-Step Guide

#### 1. Basic Setup
1. **Start Terraria** and load into a world
2. **Open Cheat Engine** as Administrator
3. **Click the computer icon** and select "Terraria.exe" from the process list
4. **Take note** of your current health, mana, coins, etc.

#### 2. Finding Health Address

1. In Cheat Engine, enter your **current health value** in the "Value" field
2. Set "Value Type" to **4 Bytes** (integers are usually 4 bytes)
3. Click **"First Scan"**
4. Go back to Terraria and **take damage** (let a slime hit you)
5. Return to Cheat Engine, enter your **new health value**, and click **"Next Scan"**
6. Repeat steps 4-5 until you have **5 or fewer results**
7. Double-click each result to add it to the address list
8. Test by changing the value - your health should change in-game
9. **Right-click the working address** and select **"Pointer scan for this address"**
10. Use the pointer scan to find a **stable address** that survives game restarts

#### 3. Finding Other Values

Repeat the same process for:
- **Mana**: Cast spells to change mana values
- **Coins**: Spend or collect coins to change values
- **Position**: Move your character and search for float values (Value Type: Float)
- **Time**: Watch the game time change (usually a float between 0.0 and 1.0)

### Important Tips

- **Use pointer scans** to find stable addresses that work after restarting
- **Float values** are used for positions, time, and percentages
- **Integer values** (4 bytes) are used for health, mana, coins, and counts
- **Take your time** - rushing will lead to unstable addresses
- **Test thoroughly** before adding to your configuration

## 🔧 Updating Memory Addresses

Once you've found working addresses in Cheat Engine:

### Method 1: Edit TerrariaAddresses.cs

1. Open `Game/TerrariaAddresses.cs`
2. Replace the placeholder offsets with your findings:

```csharp
public static class Health
{
    public static readonly long CurrentOffset = 0x00YOUR_ADDRESS; // Replace this
    public static readonly long MaximumOffset = 0x00YOUR_MAX_ADDR; // Replace this
}
```

### Method 2: Use Configuration File (Advanced)

1. Run the program once to generate a config file
2. Edit `%APPDATA%/TerrariaTweaker/config.json`
3. Add your custom addresses:

```json
{
  "CustomAddresses": {
    "HealthOffset": 123456789,
    "ManaOffset": 987654321
  }
}
```

## 🚀 Usage Guide

### First Run

1. **Build and run** the project as Administrator:
   ```bash
   dotnet run --configuration Release
   ```

2. **Start Terraria** and load into a world

3. The program will **automatically detect** and attach to Terraria

4. The **status bar** will show "Connected to Terraria" when successful

### Using the Interface

#### Player Tab
- **Set Health/Mana**: Enter a value and click "Set"
- **Refill**: Instantly restore to maximum
- **Freeze**: Lock the value so it can't decrease

#### Inventory Tab
- **Coin Management**: Set individual coin types
- **Give Max Money**: Instantly get maximum coins

#### World Tab
- **Time Controls**: Set specific times or freeze day/night cycle
- **Time Presets**: Quick buttons for dawn, noon, night

#### Teleport Tab
- **Current Position**: View your coordinates
- **Teleport**: Enter coordinates and click "Teleport"
- **Freeze Position**: Lock your character in place

#### Settings Tab
- **Disable All Hacks**: Emergency button to turn off all cheats
- **About**: Information and instructions
- **Debug Info**: View current memory values

### Hotkeys (Framework)

While the hotkey system is included, it requires additional implementation:
- **F1**: Toggle UI visibility (planned)
- **F2**: Quick health refill (planned)
- **F3**: Quick mana refill (planned)
- **F12**: Emergency disable all hacks (planned)

## 📁 Project Structure

```
TerrariaTweaker/
├── Core/                    # Memory manipulation core
│   ├── MemoryManager.cs     # Windows API memory operations
│   ├── ProcessHelper.cs     # Process detection and management
│   └── MemoryPatcher.cs     # Value freezing and patching
├── Game/                    # Game-specific implementations
│   ├── TerrariaAddresses.cs # Memory addresses and offsets
│   └── TerrariaHacks.cs     # High-level hack functions
├── UI/                      # User interface
│   ├── UIRenderer.cs        # ImGui rendering system
│   └── MainUI.cs            # Main UI implementation
├── Utils/                   # Utilities and configuration
│   ├── AppConfig.cs         # Configuration management
│   └── Logger.cs            # Logging system
└── Program.cs               # Main entry point
```

## 🔧 Configuration

The application creates a configuration file at:
`%APPDATA%/TerrariaTweaker/config.json`

### Key Settings

```json
{
  "TargetProcessName": "Terraria",
  "AutoAttach": true,
  "ProcessCheckInterval": 2000,
  "WindowWidth": 800,
  "WindowHeight": 600,
  "ShowConfirmationDialogs": true,
  "DefaultHealthValue": 400,
  "DefaultManaValue": 200
}
```

## 🔍 Troubleshooting

### Common Issues

#### "Not connected to Terraria"
- Make sure Terraria is running
- Run the hack menu as Administrator
- Check that the process name matches ("Terraria" without .exe)

#### "Memory operations failed"
- Ensure you're running as Administrator
- Verify your memory addresses are correct
- Check that Terraria hasn't updated (addresses may have changed)

#### Values not changing in game
- Double-check your memory addresses in Cheat Engine
- Make sure you're using pointer scans for stable addresses
- Verify the data types are correct (int vs float)

#### UI not appearing
- Check graphics drivers are up to date
- Try running in compatibility mode
- Verify .NET 8.0 is installed

### Debug Mode

For detailed logging, set the log level in the configuration:

```json
{
  "MinimumLogLevel": 0
}
```

Logs are saved to: `%APPDATA%/TerrariaTweaker/Logs/`

## 🛡️ Security Considerations

### Antivirus Warnings
Memory manipulation tools often trigger antivirus warnings. This is normal but:
- Only download from trusted sources
- Scan the code yourself if concerned
- Add exceptions to your antivirus if needed

### Safe Usage
- **Always backup saves** before using
- **Test in new worlds** first
- **Don't use in multiplayer** - it's unfair and may get you banned
- **Keep memory addresses private** - sharing them violates game ToS

## 🎓 Learning Resources

### Understanding Memory Manipulation
- [Cheat Engine Tutorial](https://wiki.cheatengine.org/index.php?title=Tutorials)
- [Memory Scanning Basics](https://guidedhacking.com/threads/how-to-hack-any-game-first-steps-beginners-guide.5661/)
- [Pointer Chains Explained](https://wiki.cheatengine.org/index.php?title=Pointer_scan)

### C# and Windows API
- [P/Invoke Tutorial](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [Windows Memory Management](https://docs.microsoft.com/en-us/windows/win32/memory/memory-management)
- [ImGui.NET Documentation](https://github.com/mellinoe/ImGui.NET)

## 📋 TODO / Future Improvements

### High Priority
- [ ] Complete ImGui rendering implementation
- [ ] Add hotkey system using global hooks
- [ ] Implement item ID scanning and manipulation
- [ ] Add support for more games

### Medium Priority
- [ ] Create address auto-detection system
- [ ] Add backup/restore save functionality
- [ ] Implement plugin system for extensions
- [ ] Add network detection to prevent multiplayer use

### Low Priority
- [ ] Create GUI for easier address configuration
- [ ] Add scripting support for custom cheats
- [ ] Implement theme customization
- [ ] Add sound effects and notifications

## 🤝 Contributing

This is an educational project! Contributions are welcome:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/AmazingFeature`
3. **Commit your changes**: `git commit -m 'Add some AmazingFeature'`
4. **Push to the branch**: `git push origin feature/AmazingFeature`
5. **Open a Pull Request**

### Contribution Guidelines
- Follow existing code style and patterns
- Add comprehensive comments for educational value
- Include unit tests where possible
- Update documentation for new features
- Ensure compatibility with latest Terraria version

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Re-Logic** for creating Terraria
- **Cheat Engine Team** for the amazing reverse engineering tool
- **ImGui** developers for the excellent immediate mode GUI
- **Veldrid** team for the graphics abstraction layer
- The **game hacking community** for sharing knowledge and techniques

## ⚖️ Legal Notice

This software is provided for educational purposes only. The authors do not encourage or endorse:
- Cheating in online/multiplayer games
- Circumventing game copy protection
- Violating terms of service
- Any illegal activities

Users are responsible for complying with all applicable laws and game terms of service.

---

**Happy Learning!** 🎮✨

*Remember: The best hackers are also the best at understanding how things work. Use this knowledge responsibly and always keep learning!*