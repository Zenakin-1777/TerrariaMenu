using System;

namespace TerrariaTweaker.Game
{
    /// <summary>
    /// Contains memory addresses and offsets for Terraria game values
    /// These are placeholder values that users should replace with addresses found using Cheat Engine
    /// 
    /// HOW TO USE CHEAT ENGINE TO FIND ADDRESSES:
    /// 1. Start Terraria and load into a world
    /// 2. Open Cheat Engine and attach to the Terraria process
    /// 3. Search for the value you want to modify (e.g., current health)
    /// 4. Change the value in-game and search again for the new value
    /// 5. Repeat until you find the exact memory address
    /// 6. Use pointer scans to find stable addresses that work after game restart
    /// 7. Replace the placeholder addresses below with your findings
    /// </summary>
    public static class TerrariaAddresses
    {
        #region Base Address Information
        
        /// <summary>
        /// Base address of the main Terraria executable
        /// This will be automatically detected at runtime
        /// </summary>
        public static IntPtr BaseAddress { get; set; } = IntPtr.Zero;
        
        /// <summary>
        /// Example of a stable base address pattern for Terraria
        /// Format: "Terraria.exe"+[hex_offset]
        /// This is a placeholder - find the actual stable base using Cheat Engine
        /// </summary>
        public static readonly long BaseOffset = 0x00C0A123; // PLACEHOLDER - Replace with real offset
        
        #endregion

        #region Player Stats Addresses
        
        /// <summary>
        /// Player health (current/maximum)
        /// PLACEHOLDER ADDRESSES - Find these using Cheat Engine:
        /// 1. Start with full health, search for max health value
        /// 2. Take damage, search for current health
        /// 3. Use pointer scan to find stable addresses
        /// </summary>
        public static class Health
        {
            public static readonly long CurrentOffset = 0x001234AB; // PLACEHOLDER
            public static readonly long MaximumOffset = 0x001234AF; // PLACEHOLDER
        }
        
        /// <summary>
        /// Player mana (current/maximum)
        /// Search method similar to health
        /// </summary>
        public static class Mana
        {
            public static readonly long CurrentOffset = 0x001234B3; // PLACEHOLDER
            public static readonly long MaximumOffset = 0x001234B7; // PLACEHOLDER
        }
        
        #endregion

        #region Inventory and Items
        
        /// <summary>
        /// Coin amounts (copper, silver, gold, platinum)
        /// Search for your current coin count, spend some coins, search again
        /// </summary>
        public static class Coins
        {
            public static readonly long CopperOffset = 0x001235AA; // PLACEHOLDER
            public static readonly long SilverOffset = 0x001235AB; // PLACEHOLDER
            public static readonly long GoldOffset = 0x001235AC; // PLACEHOLDER
            public static readonly long PlatinumOffset = 0x001235AD; // PLACEHOLDER
        }
        
        /// <summary>
        /// Selected item information
        /// Find by selecting an item with a specific quantity/type
        /// </summary>
        public static class SelectedItem
        {
            public static readonly long QuantityOffset = 0x001236CC; // PLACEHOLDER
            public static readonly long ItemIdOffset = 0x001236D0; // PLACEHOLDER
        }
        
        #endregion

        #region Player Position and Movement
        
        /// <summary>
        /// Player world coordinates
        /// These are usually float values (32-bit floating point)
        /// Move your character and search for changing coordinate values
        /// </summary>
        public static class Position
        {
            public static readonly long XCoordinateOffset = 0x001237AA; // PLACEHOLDER
            public static readonly long YCoordinateOffset = 0x001237AE; // PLACEHOLDER
        }
        
        /// <summary>
        /// Player velocity (movement speed)
        /// Jump or move quickly to find these changing values
        /// </summary>
        public static class Velocity
        {
            public static readonly long XVelocityOffset = 0x001238BB; // PLACEHOLDER
            public static readonly long YVelocityOffset = 0x001238BF; // PLACEHOLDER
        }
        
        #endregion

        #region Combat and Defense
        
        /// <summary>
        /// Defense value
        /// Change armor to modify defense, then search for the value
        /// </summary>
        public static readonly long DefenseOffset = 0x001239CC; // PLACEHOLDER
        
        /// <summary>
        /// Attack damage modifier
        /// This might be a percentage or flat bonus
        /// </summary>
        public static readonly long DamageOffset = 0x00123ADD; // PLACEHOLDER
        
        #endregion

        #region Game State
        
        /// <summary>
        /// Time of day (useful for freezing day/night cycle)
        /// Watch the time change and search for the corresponding value
        /// Usually stored as a float between 0.0 and 1.0
        /// </summary>
        public static readonly long TimeOffset = 0x00123BEE; // PLACEHOLDER
        
        /// <summary>
        /// Weather effects (rain intensity, etc.)
        /// Wait for weather to change and search for the values
        /// </summary>
        public static readonly long WeatherOffset = 0x00123CFF; // PLACEHOLDER
        
        #endregion

        #region Pointer Chain Examples
        
        /// <summary>
        /// Example of a multi-level pointer chain for player data
        /// Many game values are accessed through pointer chains like this:
        /// BaseAddress -> Offset1 -> Offset2 -> Final Value
        /// Use Cheat Engine's pointer scan feature to find these
        /// </summary>
        public static class PointerChains
        {
            /// <summary>
            /// Example pointer chain for player health
            /// Replace with actual offsets found through pointer scanning
            /// </summary>
            public static readonly long[] HealthChain = { 0x00AB1234, 0x50, 0x18, 0x4 }; // PLACEHOLDER
            
            /// <summary>
            /// Example pointer chain for inventory
            /// </summary>
            public static readonly long[] InventoryChain = { 0x00CD5678, 0x8, 0x10, 0x0 }; // PLACEHOLDER
        }
        
        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Calculates an absolute address from the base address and offset
        /// </summary>
        /// <param name="offset">Offset from base address</param>
        /// <returns>Calculated absolute address</returns>
        public static IntPtr GetAddress(long offset)
        {
            if (BaseAddress == IntPtr.Zero)
            {
                throw new InvalidOperationException("Base address not set. Make sure the game process is attached.");
            }
            
            return new IntPtr(BaseAddress.ToInt64() + offset);
        }
        
        /// <summary>
        /// Validates that the base address has been set
        /// </summary>
        /// <returns>True if base address is valid</returns>
        public static bool IsInitialized()
        {
            return BaseAddress != IntPtr.Zero;
        }
        
        #endregion
    }

    /// <summary>
    /// Common item IDs for Terraria
    /// These can be used for inventory manipulation
    /// Find the actual IDs by examining items in your inventory with Cheat Engine
    /// </summary>
    public static class TerrariaItems
    {
        // Weapons (PLACEHOLDER VALUES - Find actual IDs)
        public const int CopperSword = 1;
        public const int IronSword = 2;
        public const int GoldSword = 3;
        public const int TerraBlade = 757; // Example of known item ID
        
        // Tools (PLACEHOLDER VALUES)
        public const int CopperPickaxe = 10;
        public const int IronPickaxe = 11;
        public const int GoldPickaxe = 12;
        
        // Potions (PLACEHOLDER VALUES)
        public const int HealthPotion = 20;
        public const int ManaPotion = 21;
        public const int IronskinPotion = 22;
        
        // Materials (PLACEHOLDER VALUES)
        public const int Wood = 30;
        public const int Stone = 31;
        public const int IronOre = 32;
        public const int GoldOre = 33;
    }
}