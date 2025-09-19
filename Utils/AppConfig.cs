using System;
using System.IO;
using Newtonsoft.Json;

namespace TerrariaTweaker.Utils
{
    /// <summary>
    /// Configuration settings for the Terraria Tweaker application
    /// Handles loading, saving, and managing user preferences
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Path where the configuration file will be stored
        /// Located in the user's AppData folder for persistence across updates
        /// </summary>
        public static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TerrariaTweaker",
            "config.json");

        #region UI Settings

        /// <summary>
        /// Whether to start with the main window visible
        /// </summary>
        public bool StartWindowVisible { get; set; } = true;

        /// <summary>
        /// Initial window width
        /// </summary>
        public int WindowWidth { get; set; } = 800;

        /// <summary>
        /// Initial window height
        /// </summary>
        public int WindowHeight { get; set; } = 600;

        /// <summary>
        /// Window position X coordinate (saved when user moves window)
        /// </summary>
        public int WindowX { get; set; } = 50;

        /// <summary>
        /// Window position Y coordinate
        /// </summary>
        public int WindowY { get; set; } = 50;

        /// <summary>
        /// UI scale factor for high DPI displays
        /// </summary>
        public float UIScale { get; set; } = 1.0f;

        /// <summary>
        /// Selected UI theme (0 = Dark, 1 = Light, 2 = Custom)
        /// </summary>
        public int Theme { get; set; } = 0;

        #endregion

        #region Process Settings

        /// <summary>
        /// Name of the process to attach to (without .exe extension)
        /// </summary>
        public string TargetProcessName { get; set; } = "Terraria";

        /// <summary>
        /// Whether to automatically attach to the process when found
        /// </summary>
        public bool AutoAttach { get; set; } = true;

        /// <summary>
        /// How often to check for the target process (in milliseconds)
        /// </summary>
        public int ProcessCheckInterval { get; set; } = 2000;

        #endregion

        #region Memory Addresses

        /// <summary>
        /// Custom memory address overrides
        /// Users can modify these without recompiling if they find better addresses
        /// </summary>
        public CustomAddresses CustomAddresses { get; set; } = new CustomAddresses();

        #endregion

        #region Default Values

        /// <summary>
        /// Default health value for setting and freezing
        /// </summary>
        public int DefaultHealthValue { get; set; } = 400;

        /// <summary>
        /// Default mana value for setting and freezing
        /// </summary>
        public int DefaultManaValue { get; set; } = 200;

        /// <summary>
        /// Default coin amounts for quick money feature
        /// </summary>
        public CoinDefaults DefaultCoins { get; set; } = new CoinDefaults();

        #endregion

        #region Hotkeys

        /// <summary>
        /// Hotkey settings for quick access to features
        /// </summary>
        public HotkeySettings Hotkeys { get; set; } = new HotkeySettings();

        #endregion

        #region Safety Settings

        /// <summary>
        /// Whether to show confirmation dialogs for dangerous operations
        /// </summary>
        public bool ShowConfirmationDialogs { get; set; } = true;

        /// <summary>
        /// Whether to create backup saves before applying major changes
        /// </summary>
        public bool CreateBackups { get; set; } = true;

        /// <summary>
        /// Maximum number of backup files to keep
        /// </summary>
        public int MaxBackups { get; set; } = 5;

        #endregion

        #region Methods

        /// <summary>
        /// Loads the configuration from file
        /// Creates default configuration if file doesn't exist
        /// </summary>
        /// <returns>Loaded configuration object</returns>
        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var config = JsonConvert.DeserializeObject<AppConfig>(json);
                    
                    if (config != null)
                    {
                        Console.WriteLine("Configuration loaded successfully");
                        return config;
                    }
                }
                
                Console.WriteLine("Creating new configuration file");
                var defaultConfig = new AppConfig();
                defaultConfig.Save();
                return defaultConfig;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                Console.WriteLine("Using default configuration");
                return new AppConfig();
            }
        }

        /// <summary>
        /// Saves the current configuration to file
        /// </summary>
        /// <returns>True if save was successful</returns>
        public bool Save()
        {
            try
            {
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(ConfigPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serialize to JSON with formatting
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string json = JsonConvert.SerializeObject(this, settings);
                File.WriteAllText(ConfigPath, json);
                
                Console.WriteLine($"Configuration saved to {ConfigPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resets all settings to their default values
        /// </summary>
        public void ResetToDefaults()
        {
            var defaultConfig = new AppConfig();
            
            // Copy all properties from default config
            StartWindowVisible = defaultConfig.StartWindowVisible;
            WindowWidth = defaultConfig.WindowWidth;
            WindowHeight = defaultConfig.WindowHeight;
            WindowX = defaultConfig.WindowX;
            WindowY = defaultConfig.WindowY;
            UIScale = defaultConfig.UIScale;
            Theme = defaultConfig.Theme;
            TargetProcessName = defaultConfig.TargetProcessName;
            AutoAttach = defaultConfig.AutoAttach;
            ProcessCheckInterval = defaultConfig.ProcessCheckInterval;
            CustomAddresses = new CustomAddresses();
            DefaultHealthValue = defaultConfig.DefaultHealthValue;
            DefaultManaValue = defaultConfig.DefaultManaValue;
            DefaultCoins = new CoinDefaults();
            Hotkeys = new HotkeySettings();
            ShowConfirmationDialogs = defaultConfig.ShowConfirmationDialogs;
            CreateBackups = defaultConfig.CreateBackups;
            MaxBackups = defaultConfig.MaxBackups;

            Console.WriteLine("Configuration reset to defaults");
        }

        /// <summary>
        /// Validates the current configuration and fixes any invalid values
        /// </summary>
        public void Validate()
        {
            // Clamp window size to reasonable values
            WindowWidth = Math.Max(400, Math.Min(3840, WindowWidth));
            WindowHeight = Math.Max(300, Math.Min(2160, WindowHeight));
            
            // Clamp UI scale
            UIScale = Math.Max(0.5f, Math.Min(3.0f, UIScale));
            
            // Validate theme index
            if (Theme < 0 || Theme > 2) Theme = 0;
            
            // Validate process check interval
            ProcessCheckInterval = Math.Max(500, Math.Min(30000, ProcessCheckInterval));
            
            // Validate default values
            DefaultHealthValue = Math.Max(1, Math.Min(9999, DefaultHealthValue));
            DefaultManaValue = Math.Max(1, Math.Min(9999, DefaultManaValue));
            
            // Validate backup settings
            MaxBackups = Math.Max(0, Math.Min(20, MaxBackups));
        }

        #endregion
    }

    /// <summary>
    /// Custom memory address overrides
    /// </summary>
    public class CustomAddresses
    {
        /// <summary>
        /// Override for health address offset
        /// Set to null to use default from TerrariaAddresses
        /// </summary>
        public long? HealthOffset { get; set; } = null;

        /// <summary>
        /// Override for maximum health address offset
        /// </summary>
        public long? MaxHealthOffset { get; set; } = null;

        /// <summary>
        /// Override for mana address offset
        /// </summary>
        public long? ManaOffset { get; set; } = null;

        /// <summary>
        /// Override for maximum mana address offset
        /// </summary>
        public long? MaxManaOffset { get; set; } = null;

        /// <summary>
        /// Override for X coordinate address offset
        /// </summary>
        public long? PositionXOffset { get; set; } = null;

        /// <summary>
        /// Override for Y coordinate address offset
        /// </summary>
        public long? PositionYOffset { get; set; } = null;

        /// <summary>
        /// Override for coin address offsets
        /// </summary>
        public long? CopperCoinOffset { get; set; } = null;
        public long? SilverCoinOffset { get; set; } = null;
        public long? GoldCoinOffset { get; set; } = null;
        public long? PlatinumCoinOffset { get; set; } = null;
    }

    /// <summary>
    /// Default coin amounts for quick money features
    /// </summary>
    public class CoinDefaults
    {
        public int Copper { get; set; } = 999;
        public int Silver { get; set; } = 999;
        public int Gold { get; set; } = 999;
        public int Platinum { get; set; } = 999;
    }

    /// <summary>
    /// Hotkey configuration
    /// Note: Full hotkey implementation would require additional libraries
    /// This provides the structure for future implementation
    /// </summary>
    public class HotkeySettings
    {
        /// <summary>
        /// Toggle UI visibility hotkey (e.g., "F1")
        /// </summary>
        public string ToggleUIHotkey { get; set; } = "F1";

        /// <summary>
        /// Emergency disable all hacks hotkey
        /// </summary>
        public string DisableAllHotkey { get; set; } = "F12";

        /// <summary>
        /// Quick health refill hotkey
        /// </summary>
        public string RefillHealthHotkey { get; set; } = "F2";

        /// <summary>
        /// Quick mana refill hotkey
        /// </summary>
        public string RefillManaHotkey { get; set; } = "F3";

        /// <summary>
        /// Whether hotkeys are enabled
        /// </summary>
        public bool EnableHotkeys { get; set; } = true;

        /// <summary>
        /// Whether hotkeys work when the game window has focus
        /// </summary>
        public bool GlobalHotkeys { get; set; } = false;
    }
}