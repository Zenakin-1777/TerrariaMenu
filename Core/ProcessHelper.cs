using System;
using System.Diagnostics;
using System.Linq;

namespace TerrariaTweaker.Core
{
    /// <summary>
    /// Helper class for process management and detection
    /// Handles finding game processes and validating process states
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Finds a running process by its name (without .exe extension)
        /// Returns null if the process is not found or if multiple processes exist
        /// </summary>
        /// <param name="processName">Name of the process to find (e.g., "Terraria")</param>
        /// <returns>Process instance or null if not found/multiple instances</returns>
        public static Process? FindGameProcess(string processName)
        {
            try
            {
                // Get all processes with the specified name
                Process[] processes = Process.GetProcessesByName(processName);
                
                if (processes.Length == 0)
                {
                    return null; // No process found
                }
                
                if (processes.Length > 1)
                {
                    // Multiple processes found - this could be problematic for memory hacking
                    // For now, we'll return the first one, but this should be handled better in production
                    Console.WriteLine($"Warning: Multiple instances of {processName} found. Using the first one.");
                }
                
                return processes[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding process {processName}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Checks if a process is still running and accessible
        /// Used to verify that our target process hasn't closed or become inaccessible
        /// </summary>
        /// <param name="process">Process to validate</param>
        /// <returns>True if process is valid and running</returns>
        public static bool IsProcessValid(Process? process)
        {
            if (process == null)
                return false;
                
            try
            {
                // Check if process has exited
                if (process.HasExited)
                    return false;
                    
                // Try to access a basic property to ensure we still have access
                _ = process.ProcessName;
                return true;
            }
            catch
            {
                // Any exception means the process is no longer accessible
                return false;
            }
        }
        
        /// <summary>
        /// Gets the base address of the main module of a process
        /// This is often used as a reference point for calculating memory addresses
        /// </summary>
        /// <param name="process">Target process</param>
        /// <returns>Base address of the main module or IntPtr.Zero if failed</returns>
        public static IntPtr GetMainModuleBaseAddress(Process process)
        {
            try
            {
                if (!IsProcessValid(process))
                    return IntPtr.Zero;
                    
                return process.MainModule?.BaseAddress ?? IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting base address: {ex.Message}");
                return IntPtr.Zero;
            }
        }
        
        /// <summary>
        /// Attempts to find a module by name within the target process
        /// Useful for games that load specific DLLs we want to target
        /// </summary>
        /// <param name="process">Target process</param>
        /// <param name="moduleName">Name of the module to find (e.g., "GameEngine.dll")</param>
        /// <returns>ProcessModule if found, null otherwise</returns>
        public static ProcessModule? FindModule(Process process, string moduleName)
        {
            try
            {
                if (!IsProcessValid(process))
                    return null;
                    
                return process.Modules.Cast<ProcessModule>()
                    .FirstOrDefault(m => string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding module {moduleName}: {ex.Message}");
                return null;
            }
        }
    }
}