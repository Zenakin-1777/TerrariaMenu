using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using TerrariaTweaker.Core;
using TerrariaTweaker.Game;
using TerrariaTweaker.UI;
using TerrariaTweaker.Utils;

namespace TerrariaTweaker
{
    /// <summary>
    /// Main program class for the Terraria Tweaker application
    /// Initializes all components and manages the application lifecycle
    /// </summary>
    class Program
    {
        private static UIRenderer? uiRenderer;
        private static MemoryManager? memoryManager;
        private static TerrariaHacks? hacks;
        private static MainUI? mainUI;
        private static AppConfig? config;
        private static CancellationTokenSource? cancellationTokenSource;
        private static Task? processMonitorTask;

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            // Initialize logging system
            Logger.Info("=== Terraria Tweaker Starting ===", "Application");
            Logger.Info($"Version: 1.0.0", "Application");
            Logger.Info($"Build Date: {DateTime.Now:yyyy-MM-dd}", "Application");

            try
            {
                // Check if running as administrator (Windows only)
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    CheckAdministratorPrivileges();
                }

                // Load configuration
                config = AppConfig.Load();
                config.Validate();

                Logger.Info("Configuration loaded successfully", "Application");

                // Initialize core components
                if (!InitializeComponents())
                {
                    Logger.Error("Failed to initialize core components", "Application");
                    return;
                }

                // Start process monitoring
                StartProcessMonitoring();

                // Run main application loop
                RunApplication();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Fatal error in main application", "Application");
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            finally
            {
                // Cleanup
                Shutdown();
            }
        }

        /// <summary>
        /// Checks if the application is running with administrator privileges
        /// Memory manipulation requires elevated permissions
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static void CheckAdministratorPrivileges()
        {
            try
            {
                // Only check admin privileges on Windows
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    Logger.Info("Administrator privilege check skipped on non-Windows platform", "Security");
                    return;
                }

                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

                if (!isAdmin)
                {
                    Logger.Warning("Application is not running as Administrator", "Security");
                    Logger.Warning("Memory operations may fail without elevated privileges", "Security");
                    Console.WriteLine("WARNING: Not running as Administrator. Memory operations may fail.");
                    Console.WriteLine("Consider running as Administrator for full functionality.");
                }
                else
                {
                    Logger.Info("Running with Administrator privileges", "Security");
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Failed to check administrator privileges", "Security");
            }
        }

        /// <summary>
        /// Initializes all application components
        /// </summary>
        /// <returns>True if initialization succeeded</returns>
        private static bool InitializeComponents()
        {
            try
            {
                Logger.Info("Initializing components...", "Application");

                // Initialize memory manager
                memoryManager = new MemoryManager();
                Logger.Info("Memory manager created", "Application");

                // Initialize game hacks system
                hacks = new TerrariaHacks(memoryManager);
                Logger.Info("Game hacks system created", "Application");

                // Initialize UI renderer
                uiRenderer = new UIRenderer();
                if (!uiRenderer.Initialize("Terraria Tweaker", config!.WindowWidth, config.WindowHeight))
                {
                    Logger.Error("Failed to initialize UI renderer", "UI");
                    return false;
                }

                Logger.Info("UI renderer initialized", "UI");

                // Initialize main UI
                mainUI = new MainUI(hacks);
                
                // Connect UI renderer to main UI
                uiRenderer.RenderUI += mainUI.Render;

                Logger.Info("Main UI initialized", "UI");

                // Set initial window visibility
                uiRenderer.WindowVisible = config.StartWindowVisible;

                Logger.Info("All components initialized successfully", "Application");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Failed to initialize components", "Application");
                return false;
            }
        }

        /// <summary>
        /// Starts background process monitoring to automatically attach to Terraria
        /// </summary>
        private static void StartProcessMonitoring()
        {
            if (config?.AutoAttach != true)
            {
                Logger.Info("Auto-attach disabled in configuration", "Process");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            processMonitorTask = Task.Run(() => ProcessMonitorLoop(cancellationTokenSource.Token));
            
            Logger.Info("Process monitoring started", "Process");
        }

        /// <summary>
        /// Background loop that monitors for the target process
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for graceful shutdown</param>
        private static async Task ProcessMonitorLoop(CancellationToken cancellationToken)
        {
            Logger.Info($"Monitoring for process: {config!.TargetProcessName}", "Process");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Check if we're already attached to a valid process
                    if (memoryManager!.IsAttached)
                    {
                        // Wait longer when already attached
                        await Task.Delay(config.ProcessCheckInterval * 2, cancellationToken);
                        continue;
                    }

                    // Look for the target process
                    var process = ProcessHelper.FindGameProcess(config.TargetProcessName);
                    
                    if (process != null)
                    {
                        Logger.Info($"Found {config.TargetProcessName} process (PID: {process.Id})", "Process");

                        // Attempt to attach
                        if (memoryManager.AttachToProcess(process))
                        {
                            // Set the base address for memory calculations
                            var baseAddress = ProcessHelper.GetMainModuleBaseAddress(process);
                            if (baseAddress != IntPtr.Zero)
                            {
                                TerrariaAddresses.BaseAddress = baseAddress;
                                Logger.Info($"Base address set: 0x{baseAddress:X}", "Process");
                            }
                            else
                            {
                                Logger.Warning("Could not get base address", "Process");
                            }

                            Logger.Info("Successfully attached to game process", "Process");
                        }
                        else
                        {
                            Logger.Error("Failed to attach to game process", "Process");
                        }
                    }

                    // Wait before next check
                    await Task.Delay(config.ProcessCheckInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Error in process monitor loop", "Process");
                    await Task.Delay(config.ProcessCheckInterval, cancellationToken);
                }
            }

            Logger.Info("Process monitoring stopped", "Process");
        }

        /// <summary>
        /// Main application loop that handles UI rendering and input
        /// </summary>
        private static void RunApplication()
        {
            Logger.Info("Starting main application loop", "Application");

            while (uiRenderer!.RenderFrame())
            {
                // The rendering loop handles everything
                // Additional per-frame logic could go here if needed
                
                // Small delay to prevent excessive CPU usage
                Thread.Sleep(16); // ~60 FPS
            }

            Logger.Info("Main application loop ended", "Application");
        }

        /// <summary>
        /// Performs cleanup when the application is shutting down
        /// </summary>
        private static void Shutdown()
        {
            Logger.Info("Starting application shutdown", "Application");

            try
            {
                // Stop process monitoring
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    
                    if (processMonitorTask != null)
                    {
                        processMonitorTask.Wait(5000); // Wait up to 5 seconds
                        processMonitorTask.Dispose();
                    }
                    
                    cancellationTokenSource.Dispose();
                }

                // Save configuration
                if (config != null)
                {
                    config.Save();
                }

                // Dispose of components in reverse order
                hacks?.Dispose();
                memoryManager?.Dispose();
                uiRenderer?.Dispose();

                Logger.Info("All components disposed", "Application");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Error during shutdown", "Application");
            }
            finally
            {
                Logger.Shutdown();
            }
        }

        /// <summary>
        /// Handles console control events (Ctrl+C, window close, etc.)
        /// Ensures graceful shutdown when the console window is closed
        /// </summary>
        /// <param name="ctrlType">Type of control event</param>
        /// <returns>True if handled</returns>
        private static bool ConsoleCtrlHandler(int ctrlType)
        {
            Logger.Info($"Console control event received: {ctrlType}", "Application");
            
            // Trigger shutdown
            cancellationTokenSource?.Cancel();
            
            return true; // Handled
        }

        // P/Invoke for console control handler
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        
        private delegate bool ConsoleCtrlDelegate(int CtrlType);
        
        static Program()
        {
            // Set up console control handler for graceful shutdown
            SetConsoleCtrlHandler(ConsoleCtrlHandler, true);
        }
    }

    /// <summary>
    /// Application information and constants
    /// </summary>
    public static class AppInfo
    {
        public const string Name = "Terraria Tweaker";
        public const string Version = "1.0.0";
        public const string Description = "A modern hack menu for Terraria with memory manipulation capabilities";
        public const string Author = "AI Assistant";
        public const string Website = "https://github.com/example/terraria-tweaker";

        /// <summary>
        /// Gets formatted application title
        /// </summary>
        public static string FullTitle => $"{Name} v{Version}";

        /// <summary>
        /// Gets application build information
        /// </summary>
        public static string BuildInfo => $"Built on {DateTime.Now:yyyy-MM-dd} with .NET 8.0";
    }
}
