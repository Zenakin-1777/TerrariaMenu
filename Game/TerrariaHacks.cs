using System;
using TerrariaTweaker.Core;

namespace TerrariaTweaker.Game
{
    /// <summary>
    /// Contains all the specific hack implementations for Terraria
    /// This class provides high-level methods for common cheats and modifications
    /// </summary>
    public class TerrariaHacks : IDisposable
    {
        private readonly MemoryManager memoryManager;
        private readonly MemoryPatcher memoryPatcher;
        private bool disposed = false;

        /// <summary>
        /// Gets whether the hack system is currently attached to Terraria
        /// </summary>
        public bool IsAttached => memoryManager.IsAttached;

        /// <summary>
        /// Gets the current health values from memory
        /// Returns (current, maximum) or (0, 0) if failed
        /// </summary>
        public (int current, int maximum) CurrentHealth
        {
            get
            {
                if (!IsAttached || !TerrariaAddresses.IsInitialized())
                    return (0, 0);

                try
                {
                    var currentAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Health.CurrentOffset);
                    var maxAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Health.MaximumOffset);
                    
                    int current = memoryManager.ReadInt32(currentAddr);
                    int maximum = memoryManager.ReadInt32(maxAddr);
                    
                    return (current, maximum);
                }
                catch
                {
                    return (0, 0);
                }
            }
        }

        /// <summary>
        /// Gets the current mana values from memory
        /// Returns (current, maximum) or (0, 0) if failed
        /// </summary>
        public (int current, int maximum) CurrentMana
        {
            get
            {
                if (!IsAttached || !TerrariaAddresses.IsInitialized())
                    return (0, 0);

                try
                {
                    var currentAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Mana.CurrentOffset);
                    var maxAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Mana.MaximumOffset);
                    
                    int current = memoryManager.ReadInt32(currentAddr);
                    int maximum = memoryManager.ReadInt32(maxAddr);
                    
                    return (current, maximum);
                }
                catch
                {
                    return (0, 0);
                }
            }
        }

        /// <summary>
        /// Gets the current player position from memory
        /// Returns (x, y) coordinates or (0, 0) if failed
        /// </summary>
        public (float x, float y) CurrentPosition
        {
            get
            {
                if (!IsAttached || !TerrariaAddresses.IsInitialized())
                    return (0f, 0f);

                try
                {
                    var xAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Position.XCoordinateOffset);
                    var yAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Position.YCoordinateOffset);
                    
                    float x = memoryManager.ReadFloat(xAddr);
                    float y = memoryManager.ReadFloat(yAddr);
                    
                    return (x, y);
                }
                catch
                {
                    return (0f, 0f);
                }
            }
        }

        public TerrariaHacks(MemoryManager memoryManager)
        {
            this.memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            this.memoryPatcher = new MemoryPatcher(memoryManager);
        }

        #region Health Manipulation

        /// <summary>
        /// Sets the player's current health to a specific value
        /// Does not freeze the value - it can still change normally
        /// </summary>
        /// <param name="health">Health value to set</param>
        /// <returns>True if successful</returns>
        public bool SetHealth(int health)
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
            {
                Console.WriteLine("Not attached to game or addresses not initialized");
                return false;
            }

            try
            {
                var address = TerrariaAddresses.GetAddress(TerrariaAddresses.Health.CurrentOffset);
                bool success = memoryManager.WriteInt32(address, health);
                
                if (success)
                    Console.WriteLine($"Set health to {health}");
                else
                    Console.WriteLine("Failed to set health");
                    
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting health: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Refills health to maximum
        /// Reads the maximum health value and sets current health to match
        /// </summary>
        /// <returns>True if successful</returns>
        public bool RefillHealth()
        {
            var (_, maximum) = CurrentHealth;
            if (maximum <= 0)
            {
                Console.WriteLine("Could not read maximum health");
                return false;
            }

            return SetHealth(maximum);
        }

        /// <summary>
        /// Freezes health at the specified value
        /// The health will be continuously maintained at this value
        /// </summary>
        /// <param name="health">Health value to maintain</param>
        /// <returns>True if freeze was activated</returns>
        public bool FreezeHealth(int health)
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
                return false;

            try
            {
                var address = TerrariaAddresses.GetAddress(TerrariaAddresses.Health.CurrentOffset);
                memoryPatcher.AddIntegerPatch("HealthFreeze", address, health);
                return memoryPatcher.ActivatePatch("HealthFreeze");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error freezing health: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unfreezes health, allowing it to change normally
        /// </summary>
        /// <returns>True if successfully unfrozen</returns>
        public bool UnfreezeHealth()
        {
            return memoryPatcher.DeactivatePatch("HealthFreeze");
        }

        /// <summary>
        /// Checks if health is currently frozen
        /// </summary>
        /// <returns>True if health is frozen</returns>
        public bool IsHealthFrozen()
        {
            return memoryPatcher.IsPatchActive("HealthFreeze");
        }

        #endregion

        #region Mana Manipulation

        /// <summary>
        /// Sets the player's current mana to a specific value
        /// </summary>
        /// <param name="mana">Mana value to set</param>
        /// <returns>True if successful</returns>
        public bool SetMana(int mana)
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
                return false;

            try
            {
                var address = TerrariaAddresses.GetAddress(TerrariaAddresses.Mana.CurrentOffset);
                bool success = memoryManager.WriteInt32(address, mana);
                
                if (success)
                    Console.WriteLine($"Set mana to {mana}");
                    
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting mana: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Refills mana to maximum
        /// </summary>
        /// <returns>True if successful</returns>
        public bool RefillMana()
        {
            var (_, maximum) = CurrentMana;
            if (maximum <= 0)
                return false;

            return SetMana(maximum);
        }

        /// <summary>
        /// Freezes mana at the specified value
        /// </summary>
        /// <param name="mana">Mana value to maintain</param>
        /// <returns>True if freeze was activated</returns>
        public bool FreezeMana(int mana)
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
                return false;

            try
            {
                var address = TerrariaAddresses.GetAddress(TerrariaAddresses.Mana.CurrentOffset);
                memoryPatcher.AddIntegerPatch("ManaFreeze", address, mana);
                return memoryPatcher.ActivatePatch("ManaFreeze");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error freezing mana: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unfreezes mana
        /// </summary>
        public bool UnfreezeMana()
        {
            return memoryPatcher.DeactivatePatch("ManaFreeze");
        }

        /// <summary>
        /// Checks if mana is currently frozen
        /// </summary>
        public bool IsManFrozen()
        {
            return memoryPatcher.IsPatchActive("ManaFreeze");
        }

        #endregion

        #region Money Manipulation

        /// <summary>
        /// Gets the current coin amounts
        /// Returns (copper, silver, gold, platinum) or all zeros if failed
        /// </summary>
        public (int copper, int silver, int gold, int platinum) GetCoins()
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
                return (0, 0, 0, 0);

            try
            {
                var copperAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Coins.CopperOffset);
                var silverAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Coins.SilverOffset);
                var goldAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Coins.GoldOffset);
                var platinumAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Coins.PlatinumOffset);
                
                int copper = memoryManager.ReadInt32(copperAddr);
                int silver = memoryManager.ReadInt32(silverAddr);
                int gold = memoryManager.ReadInt32(goldAddr);
                int platinum = memoryManager.ReadInt32(platinumAddr);
                
                return (copper, silver, gold, platinum);
            }
            catch
            {
                return (0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Sets specific coin amounts
        /// </summary>
        /// <param name="copper">Copper coins</param>
        /// <param name="silver">Silver coins</param>
        /// <param name="gold">Gold coins</param>
        /// <param name="platinum">Platinum coins</param>
        /// <returns>True if all coin values were set successfully</returns>
        public bool SetCoins(int copper, int silver, int gold, int platinum)
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
                return false;

            try
            {
                var copperAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Coins.CopperOffset);
                var silverAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Coins.SilverOffset);
                var goldAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Coins.GoldOffset);
                var platinumAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Coins.PlatinumOffset);
                
                bool success = true;
                success &= memoryManager.WriteInt32(copperAddr, copper);
                success &= memoryManager.WriteInt32(silverAddr, silver);
                success &= memoryManager.WriteInt32(goldAddr, gold);
                success &= memoryManager.WriteInt32(platinumAddr, platinum);
                
                if (success)
                    Console.WriteLine($"Set coins: {copper}c, {silver}s, {gold}g, {platinum}p");
                    
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting coins: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gives the player a large amount of money (999 of each coin type)
        /// </summary>
        /// <returns>True if successful</returns>
        public bool GiveMaxMoney()
        {
            return SetCoins(999, 999, 999, 999);
        }

        #endregion

        #region Position and Movement

        /// <summary>
        /// Teleports the player to specific coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if successful</returns>
        public bool TeleportTo(float x, float y)
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
                return false;

            try
            {
                var xAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Position.XCoordinateOffset);
                var yAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Position.YCoordinateOffset);
                
                bool success = true;
                success &= memoryManager.WriteFloat(xAddr, x);
                success &= memoryManager.WriteFloat(yAddr, y);
                
                if (success)
                    Console.WriteLine($"Teleported to ({x}, {y})");
                    
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error teleporting: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Freezes the player position at current location
        /// </summary>
        /// <returns>True if freeze was activated</returns>
        public bool FreezePosition()
        {
            var (x, y) = CurrentPosition;
            if (x == 0 && y == 0)
                return false;

            try
            {
                var xAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Position.XCoordinateOffset);
                var yAddr = TerrariaAddresses.GetAddress(TerrariaAddresses.Position.YCoordinateOffset);
                
                memoryPatcher.AddFloatPatch("PositionFreezeX", xAddr, x);
                memoryPatcher.AddFloatPatch("PositionFreezeY", yAddr, y);
                
                bool success = true;
                success &= memoryPatcher.ActivatePatch("PositionFreezeX");
                success &= memoryPatcher.ActivatePatch("PositionFreezeY");
                
                if (success)
                    Console.WriteLine($"Position frozen at ({x}, {y})");
                    
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error freezing position: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unfreezes player position
        /// </summary>
        /// <returns>True if successfully unfrozen</returns>
        public bool UnfreezePosition()
        {
            bool success = true;
            success &= memoryPatcher.DeactivatePatch("PositionFreezeX");
            success &= memoryPatcher.DeactivatePatch("PositionFreezeY");
            return success;
        }

        /// <summary>
        /// Checks if position is currently frozen
        /// </summary>
        /// <returns>True if position is frozen</returns>
        public bool IsPositionFrozen()
        {
            return memoryPatcher.IsPatchActive("PositionFreezeX") || memoryPatcher.IsPatchActive("PositionFreezeY");
        }

        #endregion

        #region Game State Manipulation

        /// <summary>
        /// Gets the current game time (0.0 = dawn, 0.5 = noon, 1.0 = midnight)
        /// </summary>
        /// <returns>Time value between 0.0 and 1.0, or -1 if failed</returns>
        public float GetGameTime()
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
                return -1f;

            try
            {
                var address = TerrariaAddresses.GetAddress(TerrariaAddresses.TimeOffset);
                return memoryManager.ReadFloat(address);
            }
            catch
            {
                return -1f;
            }
        }

        /// <summary>
        /// Sets the game time to a specific value
        /// </summary>
        /// <param name="time">Time value (0.0 = dawn, 0.5 = noon, 1.0 = midnight)</param>
        /// <returns>True if successful</returns>
        public bool SetGameTime(float time)
        {
            if (!IsAttached || !TerrariaAddresses.IsInitialized())
                return false;

            // Clamp time value between 0.0 and 1.0
            time = Math.Max(0f, Math.Min(1f, time));

            try
            {
                var address = TerrariaAddresses.GetAddress(TerrariaAddresses.TimeOffset);
                bool success = memoryManager.WriteFloat(address, time);
                
                if (success)
                {
                    string timeDesc = time switch
                    {
                        < 0.25f => "Dawn",
                        < 0.5f => "Morning",
                        < 0.75f => "Noon",
                        _ => "Night"
                    };
                    Console.WriteLine($"Set time to {time:F2} ({timeDesc})");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting time: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Freezes the game time at current value
        /// </summary>
        /// <returns>True if freeze was activated</returns>
        public bool FreezeTime()
        {
            float currentTime = GetGameTime();
            if (currentTime < 0)
                return false;

            try
            {
                var address = TerrariaAddresses.GetAddress(TerrariaAddresses.TimeOffset);
                memoryPatcher.AddFloatPatch("TimeFreeze", address, currentTime);
                return memoryPatcher.ActivatePatch("TimeFreeze");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error freezing time: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unfreezes game time
        /// </summary>
        /// <returns>True if successfully unfrozen</returns>
        public bool UnfreezeTime()
        {
            return memoryPatcher.DeactivatePatch("TimeFreeze");
        }

        /// <summary>
        /// Checks if time is currently frozen
        /// </summary>
        /// <returns>True if time is frozen</returns>
        public bool IsTimeFrozen()
        {
            return memoryPatcher.IsPatchActive("TimeFreeze");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Deactivates all active hacks/freezes
        /// Useful for quickly disabling all cheats
        /// </summary>
        public void DisableAllHacks()
        {
            memoryPatcher.DeactivateAllPatches();
            Console.WriteLine("All hacks disabled");
        }

        /// <summary>
        /// Gets a status report of all active hacks
        /// </summary>
        /// <returns>String describing active hacks</returns>
        public string GetActiveHacksStatus()
        {
            if (!IsAttached)
                return "Not attached to game";

            int activeCount = memoryPatcher.ActivePatchCount;
            if (activeCount == 0)
                return "No active hacks";

            var status = $"{activeCount} active hack(s): ";
            var patches = new[]
            {
                ("Health", IsHealthFrozen()),
                ("Mana", IsManFrozen()),
                ("Position", IsPositionFrozen()),
                ("Time", IsTimeFrozen())
            };

            var activeHacks = new List<string>();
            foreach (var (name, isActive) in patches)
            {
                if (isActive)
                    activeHacks.Add(name);
            }

            return status + string.Join(", ", activeHacks);
        }

        #endregion

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
                    memoryPatcher?.Dispose();
                }
                disposed = true;
            }
        }

        ~TerrariaHacks()
        {
            Dispose(false);
        }

        #endregion
    }
}