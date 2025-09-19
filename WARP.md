# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

TerrariaTweaker is a C# educational game hacking tool for Terraria that demonstrates memory manipulation techniques. It uses ImGui.NET for the UI, Veldrid for rendering, and Windows API for memory operations. The project is designed for single-player use only and serves as a learning tool for reverse engineering and memory manipulation concepts.

## Common Development Commands

### Build and Run
```powershell
# Restore dependencies
dotnet restore

# Build the project (Release configuration recommended)
dotnet build --configuration Release

# Run the application (must be run as Administrator for memory operations)
dotnet run --configuration Release

# Clean build artifacts
dotnet clean
```

### Development and Debugging
```powershell
# Build in Debug mode for development
dotnet build --configuration Debug

# Run with debug output
dotnet run --configuration Debug

# Check for compilation errors only
dotnet build --no-restore --verbosity minimal
```

### Package Management
```powershell
# Add a new package
dotnet add package PackageName

# Update all packages
dotnet list package --outdated
dotnet update
```

## Architecture Overview

### Core Architecture Pattern
The application follows a layered architecture with separation of concerns:

- **Core Layer** (`Core/`): Low-level memory manipulation and Windows API interactions
- **Game Layer** (`Game/`): Terraria-specific implementations and memory addresses
- **UI Layer** (`UI/`): ImGui-based user interface and rendering
- **Utils Layer** (`Utils/`): Configuration, logging, and utility functions

### Memory Management System
The memory manipulation system is built around three key concepts:

1. **MemoryManager** (`Core/MemoryManager.cs`): Handles direct Windows API calls for reading/writing process memory
2. **TerrariaAddresses** (`Game/TerrariaAddresses.cs`): Contains memory address definitions (placeholders that users must update)
3. **TerrariaHacks** (`Game/TerrariaHacks.cs`): High-level hack implementations that use MemoryManager

### Process Attachment Flow
The application automatically monitors and attaches to the Terraria process:
1. `ProcessHelper.FindGameProcess()` locates the target process
2. `MemoryManager.AttachToProcess()` opens a handle with required permissions
3. Base address is calculated and stored in `TerrariaAddresses.BaseAddress`
4. UI reflects connection status and enables hack controls

### UI Architecture
- **UIRenderer** creates and manages the ImGui rendering context using Veldrid
- **MainUI** implements the tabbed interface with different hack categories
- Event-driven rendering system where UIRenderer fires RenderUI events
- Modern dark theme with blue accents for professional appearance

## Key Development Considerations

### Memory Address Management
- All memory addresses in `TerrariaAddresses.cs` are placeholders
- Users must use Cheat Engine to find real addresses for their Terraria version
- Addresses are version-specific and change with game updates
- The system supports both direct offsets and pointer chains

### Windows API Integration
- Application requires Administrator privileges for memory operations
- Uses P/Invoke to call Windows API functions (ReadProcessMemory, WriteProcessMemory, etc.)
- Process handle management with proper cleanup in Dispose methods

### Configuration System
- JSON-based configuration stored in `%APPDATA%\TerrariaTweaker\config.json`
- Supports custom memory address overrides without recompilation
- Automatic validation and default value restoration

### Error Handling Patterns
- Extensive try-catch blocks around all memory operations
- Graceful degradation when process is not attached
- Console logging for debugging (consider migrating to structured logging)

## Development Workflow

### Adding New Hacks
1. Define memory addresses in `TerrariaAddresses.cs`
2. Implement read/write methods in `TerrariaHacks.cs`
3. Add UI controls in appropriate tab in `MainUI.cs`
4. Test with Cheat Engine first to verify addresses work

### Memory Address Discovery Process
1. Start Terraria and load into a world
2. Open Cheat Engine and attach to Terraria.exe
3. Search for target value (health, mana, coins, etc.)
4. Change value in-game and search again with new value
5. Repeat until unique address is found
6. Use pointer scan to find stable addresses that survive restarts
7. Update `TerrariaAddresses.cs` with findings

### UI Development
- Follow existing tab structure in `MainUI.cs`
- Use `ImGui.BeginDisabled(!hacks.IsAttached)` to disable controls when not connected
- Display current values before allowing modifications
- Provide both set and freeze options for numeric values

## Code Patterns and Conventions

### Memory Operation Pattern
```csharp
public bool SetValue(int value)
{
    if (!IsAttached || !TerrariaAddresses.IsInitialized())
        return false;

    try
    {
        var address = TerrariaAddresses.GetAddress(TerrariaAddresses.SomeValueOffset);
        bool success = memoryManager.WriteInt32(address, value);
        
        if (success)
            Console.WriteLine($"Set value to {value}");
            
        return success;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error setting value: {ex.Message}");
        return false;
    }
}
```

### UI Control Pattern
```csharp
ImGui.BeginDisabled(!hacks.IsAttached);

if (ImGui.Button("Action"))
{
    hacks.PerformAction();
}

ImGui.EndDisabled();
```

### Freeze/Unfreeze Pattern
Uses `MemoryPatcher` to continuously write values at specified intervals.

## Security and Safety

### Administrator Requirements
- Memory manipulation requires elevated privileges
- Application checks and warns if not running as Administrator
- Process attachment fails without proper permissions

### Safety Mechanisms
- Confirmation dialogs for destructive operations (configurable)
- Emergency "Disable All Hacks" functionality
- Process validation before memory operations
- Graceful handling of process termination

### Educational Disclaimers
- Tool is explicitly for educational and single-player use only
- Memory addresses must be found by users (not provided)
- Emphasizes ethical use and respect for game terms of service

## Dependencies and Technologies

### Core Dependencies
- **.NET 8.0**: Modern C# features and performance
- **ImGui.NET 1.89.7.1**: Immediate mode GUI for the interface
- **Veldrid 4.9.0**: Cross-platform rendering abstraction
- **Newtonsoft.Json 13.0.3**: JSON configuration serialization

### Windows-Specific Features
- P/Invoke for Windows API memory functions
- Process enumeration and handle management
- Console control handlers for graceful shutdown

## Testing and Validation

### Manual Testing Approach
No automated testing framework is present. Testing should focus on:
- Process attachment/detachment cycles
- Memory read/write operations with known addresses
- UI responsiveness when process is/isn't available
- Configuration loading/saving
- Error handling with invalid addresses

### Validation Steps
1. Test without Terraria running (should show "Not connected")
2. Test with Terraria running but invalid addresses (operations should fail gracefully)
3. Test with valid addresses found via Cheat Engine
4. Test process restart scenarios
5. Test configuration persistence

## Important Files to Understand

### Critical for Memory Operations
- `Core/MemoryManager.cs` - Windows API wrapper for memory operations
- `Game/TerrariaAddresses.cs` - Memory address definitions (user must update)
- `Game/TerrariaHacks.cs` - High-level hack implementations

### Critical for Architecture
- `Program.cs` - Application lifecycle and component initialization
- `Core/ProcessHelper.cs` - Process detection and validation
- `Core/MemoryPatcher.cs` - Value freezing system (referenced but not included in files shown)

### Configuration and UI
- `Utils/AppConfig.cs` - Configuration management system
- `UI/MainUI.cs` - Main user interface implementation
- `UI/UIRenderer.cs` - ImGui rendering system

This architecture enables educational exploration of memory manipulation while maintaining code organization and user safety through proper error handling and security considerations.