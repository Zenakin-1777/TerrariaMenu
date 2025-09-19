using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TerrariaTweaker.Core
{
    /// <summary>
    /// Represents a memory patch that can freeze a value at a specific address
    /// This is used for maintaining constant values like infinite health or ammo
    /// </summary>
    public class MemoryPatch
    {
        public IntPtr Address { get; }
        public object Value { get; }
        public Type ValueType { get; }
        public string Name { get; }
        public bool IsActive { get; set; }

        public MemoryPatch(IntPtr address, object value, Type valueType, string name)
        {
            Address = address;
            Value = value;
            ValueType = valueType;
            Name = name;
            IsActive = false;
        }
    }

    /// <summary>
    /// Manages memory patches and provides freezing capabilities
    /// Runs a background thread to continuously write frozen values to memory
    /// </summary>
    public class MemoryPatcher : IDisposable
    {
        private readonly MemoryManager memoryManager;
        private readonly Dictionary<string, MemoryPatch> patches;
        private readonly object lockObject = new object();
        private CancellationTokenSource? cancellationTokenSource;
        private Task? patchingTask;
        private bool disposed = false;

        /// <summary>
        /// Gets the number of active patches currently being applied
        /// </summary>
        public int ActivePatchCount
        {
            get
            {
                lock (lockObject)
                {
                    int count = 0;
                    foreach (var patch in patches.Values)
                    {
                        if (patch.IsActive) count++;
                    }
                    return count;
                }
            }
        }

        /// <summary>
        /// Gets whether the patcher is currently running and applying patches
        /// </summary>
        public bool IsRunning => patchingTask != null && !patchingTask.IsCompleted;

        public MemoryPatcher(MemoryManager memoryManager)
        {
            this.memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            this.patches = new Dictionary<string, MemoryPatch>();
        }

        /// <summary>
        /// Adds or updates a memory patch for freezing an integer value
        /// </summary>
        /// <param name="name">Unique identifier for this patch</param>
        /// <param name="address">Memory address to patch</param>
        /// <param name="value">Value to maintain at the address</param>
        public void AddIntegerPatch(string name, IntPtr address, int value)
        {
            lock (lockObject)
            {
                patches[name] = new MemoryPatch(address, value, typeof(int), name);
                Console.WriteLine($"Added integer patch '{name}' at 0x{address:X} with value {value}");
            }
        }

        /// <summary>
        /// Adds or updates a memory patch for freezing a float value
        /// </summary>
        /// <param name="name">Unique identifier for this patch</param>
        /// <param name="address">Memory address to patch</param>
        /// <param name="value">Value to maintain at the address</param>
        public void AddFloatPatch(string name, IntPtr address, float value)
        {
            lock (lockObject)
            {
                patches[name] = new MemoryPatch(address, value, typeof(float), name);
                Console.WriteLine($"Added float patch '{name}' at 0x{address:X} with value {value}");
            }
        }

        /// <summary>
        /// Adds or updates a memory patch for freezing a double value
        /// </summary>
        /// <param name="name">Unique identifier for this patch</param>
        /// <param name="address">Memory address to patch</param>
        /// <param name="value">Value to maintain at the address</param>
        public void AddDoublePatch(string name, IntPtr address, double value)
        {
            lock (lockObject)
            {
                patches[name] = new MemoryPatch(address, value, typeof(double), name);
                Console.WriteLine($"Added double patch '{name}' at 0x{address:X} with value {value}");
            }
        }

        /// <summary>
        /// Activates a specific patch by name
        /// The patch will start being applied continuously while active
        /// </summary>
        /// <param name="name">Name of the patch to activate</param>
        /// <returns>True if patch was found and activated</returns>
        public bool ActivatePatch(string name)
        {
            lock (lockObject)
            {
                if (patches.TryGetValue(name, out MemoryPatch? patch))
                {
                    patch.IsActive = true;
                    Console.WriteLine($"Activated patch: {name}");
                    
                    // Start the patching task if not already running
                    StartPatchingIfNeeded();
                    return true;
                }
                
                Console.WriteLine($"Patch not found: {name}");
                return false;
            }
        }

        /// <summary>
        /// Deactivates a specific patch by name
        /// The patch will stop being applied, allowing the game to change the value normally
        /// </summary>
        /// <param name="name">Name of the patch to deactivate</param>
        /// <returns>True if patch was found and deactivated</returns>
        public bool DeactivatePatch(string name)
        {
            lock (lockObject)
            {
                if (patches.TryGetValue(name, out MemoryPatch? patch))
                {
                    patch.IsActive = false;
                    Console.WriteLine($"Deactivated patch: {name}");
                    return true;
                }
                
                Console.WriteLine($"Patch not found: {name}");
                return false;
            }
        }

        /// <summary>
        /// Removes a patch completely from the patcher
        /// This deactivates the patch and removes it from memory
        /// </summary>
        /// <param name="name">Name of the patch to remove</param>
        /// <returns>True if patch was found and removed</returns>
        public bool RemovePatch(string name)
        {
            lock (lockObject)
            {
                if (patches.Remove(name))
                {
                    Console.WriteLine($"Removed patch: {name}");
                    return true;
                }
                
                Console.WriteLine($"Patch not found: {name}");
                return false;
            }
        }

        /// <summary>
        /// Gets a list of all patch names currently registered
        /// </summary>
        /// <returns>Array of patch names</returns>
        public string[] GetPatchNames()
        {
            lock (lockObject)
            {
                return new List<string>(patches.Keys).ToArray();
            }
        }

        /// <summary>
        /// Checks if a specific patch is currently active
        /// </summary>
        /// <param name="name">Name of the patch to check</param>
        /// <returns>True if patch exists and is active</returns>
        public bool IsPatchActive(string name)
        {
            lock (lockObject)
            {
                return patches.TryGetValue(name, out MemoryPatch? patch) && patch.IsActive;
            }
        }

        /// <summary>
        /// Deactivates all currently active patches
        /// Useful for quickly disabling all cheats
        /// </summary>
        public void DeactivateAllPatches()
        {
            lock (lockObject)
            {
                foreach (var patch in patches.Values)
                {
                    patch.IsActive = false;
                }
                Console.WriteLine("Deactivated all patches");
            }
        }

        /// <summary>
        /// Removes all patches from the patcher
        /// This completely clears the patch list
        /// </summary>
        public void ClearAllPatches()
        {
            lock (lockObject)
            {
                patches.Clear();
                Console.WriteLine("Cleared all patches");
            }
        }

        /// <summary>
        /// Starts the background patching task if it's not already running
        /// This task continuously applies active patches to maintain frozen values
        /// </summary>
        private void StartPatchingIfNeeded()
        {
            if (IsRunning) return;

            cancellationTokenSource = new CancellationTokenSource();
            patchingTask = Task.Run(() => PatchingLoop(cancellationTokenSource.Token));
            Console.WriteLine("Started memory patching task");
        }

        /// <summary>
        /// Main patching loop that runs in the background
        /// Continuously applies all active patches at regular intervals
        /// </summary>
        /// <param name="cancellationToken">Token for graceful cancellation</param>
        private async Task PatchingLoop(CancellationToken cancellationToken)
        {
            const int patchingIntervalMs = 50; // Apply patches every 50ms (20 times per second)

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Check if memory manager is still attached
                    if (!memoryManager.IsAttached)
                    {
                        await Task.Delay(1000, cancellationToken); // Wait longer if not attached
                        continue;
                    }

                    // Apply all active patches
                    ApplyActivePatches();
                    
                    // Wait before next iteration
                    await Task.Delay(patchingIntervalMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in patching loop: {ex.Message}");
                    await Task.Delay(1000, cancellationToken); // Wait before retrying
                }
            }

            Console.WriteLine("Memory patching task stopped");
        }

        /// <summary>
        /// Applies all currently active patches by writing their values to memory
        /// This method is called repeatedly by the patching loop
        /// </summary>
        private void ApplyActivePatches()
        {
            lock (lockObject)
            {
                foreach (var patch in patches.Values)
                {
                    if (!patch.IsActive) continue;

                    try
                    {
                        // Write the appropriate value type to memory
                        bool success = patch.ValueType.Name switch
                        {
                            nameof(Int32) => memoryManager.WriteInt32(patch.Address, (int)patch.Value),
                            nameof(Single) => memoryManager.WriteFloat(patch.Address, (float)patch.Value),
                            nameof(Double) => memoryManager.WriteDouble(patch.Address, (double)patch.Value),
                            _ => false
                        };

                        if (!success)
                        {
                            Console.WriteLine($"Failed to apply patch: {patch.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error applying patch {patch.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Stops the background patching task
        /// </summary>
        public void StopPatching()
        {
            cancellationTokenSource?.Cancel();
            
            if (patchingTask != null)
            {
                try
                {
                    patchingTask.Wait(5000); // Wait up to 5 seconds for graceful shutdown
                }
                catch (AggregateException)
                {
                    // Task was cancelled, which is expected
                }
                
                patchingTask = null;
            }

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            
            Console.WriteLine("Stopped memory patching");
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopPatching();
                    ClearAllPatches();
                }
                disposed = true;
            }
        }

        ~MemoryPatcher()
        {
            Dispose(false);
        }

        #endregion
    }
}