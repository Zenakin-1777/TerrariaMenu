using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using TerrariaTweaker.Game;

namespace TerrariaTweaker.UI
{
    /// <summary>
    /// Main UI class that handles the rendering of all game hack controls
    /// Provides a tabbed interface with different categories of cheats and modifications
    /// </summary>
    public class MainUI
    {
        private readonly TerrariaHacks hacks;
        private bool showMainWindow = true;
        private bool showAboutWindow = false;
        
        // UI state variables for various controls
        private int healthSetValue = 100;
        private int manaSetValue = 100;
        private int copperCoins = 0;
        private int silverCoins = 0;
        private int goldCoins = 0;
        private int platinumCoins = 0;
        private float teleportX = 0f;
        private float teleportY = 0f;
        private float gameTimeValue = 0.5f;
        
        // Tab names for reference
        private readonly string[] tabNames = { "Player", "Inventory", "World", "Teleport", "Settings" };

        public MainUI(TerrariaHacks hacks)
        {
            this.hacks = hacks ?? throw new ArgumentNullException(nameof(hacks));
        }

        /// <summary>
        /// Main rendering method - call this from the UIRenderer's RenderUI event
        /// </summary>
        public void Render()
        {
            if (showMainWindow)
            {
                RenderMainWindow();
            }

            if (showAboutWindow)
            {
                RenderAboutWindow();
            }
        }

        /// <summary>
        /// Renders the main hack menu window with tabbed interface
        /// </summary>
        private void RenderMainWindow()
        {
            // Set window flags for modern appearance
            var flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;
            
            ImGui.SetNextWindowSize(new Vector2(450, 500), ImGuiCond.FirstUseEver);
            
            if (ImGui.Begin("Terraria Tweaker - Hack Menu", ref showMainWindow, flags))
            {
                RenderStatusBar();
                
                ImGui.Separator();
                
                // Render tab bar
                if (ImGui.BeginTabBar("HackTabs"))
                {
                    if (ImGui.BeginTabItem("Player"))
                    {
                        RenderPlayerTab();
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("Inventory"))
                    {
                        RenderInventoryTab();
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("World"))
                    {
                        RenderWorldTab();
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("Teleport"))
                    {
                        RenderTeleportTab();
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("Settings"))
                    {
                        RenderSettingsTab();
                        ImGui.EndTabItem();
                    }
                    
                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }

        /// <summary>
        /// Renders the status bar showing connection status and active hacks
        /// </summary>
        private void RenderStatusBar()
        {
            // Connection status indicator
            if (hacks.IsAttached)
            {
                ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "● Connected to Terraria");
            }
            else
            {
                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "● Not connected to Terraria");
            }
            
            ImGui.SameLine();
            
            // Active hacks status
            if (hacks.IsAttached)
            {
                string status = hacks.GetActiveHacksStatus();
                ImGui.TextColored(new Vector4(0.8f, 0.8f, 1.0f, 1.0f), $"| {status}");
            }
        }

        /// <summary>
        /// Renders the Player tab with health, mana, and basic player stats
        /// </summary>
        private void RenderPlayerTab()
        {
            ImGui.Text("Player Statistics & Health");
            ImGui.Separator();

            // Health section
            ImGui.Text("Health Management");
            
            if (hacks.IsAttached)
            {
                var (currentHealth, maxHealth) = hacks.CurrentHealth;
                ImGui.Text($"Current Health: {currentHealth} / {maxHealth}");
            }
            else
            {
                ImGui.TextDisabled("Current Health: Not connected");
            }

            ImGui.InputInt("Set Health Value", ref healthSetValue);
            
            ImGui.BeginDisabled(!hacks.IsAttached);
            
            if (ImGui.Button("Set Health"))
            {
                hacks.SetHealth(healthSetValue);
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Refill Health"))
            {
                hacks.RefillHealth();
            }
            
            // Health freeze checkbox
            bool healthFrozen = hacks.IsHealthFrozen();
            if (ImGui.Checkbox("Freeze Health", ref healthFrozen))
            {
                if (healthFrozen)
                {
                    hacks.FreezeHealth(healthSetValue);
                }
                else
                {
                    hacks.UnfreezeHealth();
                }
            }
            
            ImGui.EndDisabled();
            
            ImGui.Spacing();
            
            // Mana section
            ImGui.Text("Mana Management");
            
            if (hacks.IsAttached)
            {
                var (currentMana, maxMana) = hacks.CurrentMana;
                ImGui.Text($"Current Mana: {currentMana} / {maxMana}");
            }
            else
            {
                ImGui.TextDisabled("Current Mana: Not connected");
            }

            ImGui.InputInt("Set Mana Value", ref manaSetValue);
            
            ImGui.BeginDisabled(!hacks.IsAttached);
            
            if (ImGui.Button("Set Mana"))
            {
                hacks.SetMana(manaSetValue);
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Refill Mana"))
            {
                hacks.RefillMana();
            }
            
            // Mana freeze checkbox
            bool manaFrozen = hacks.IsManFrozen();
            if (ImGui.Checkbox("Freeze Mana", ref manaFrozen))
            {
                if (manaFrozen)
                {
                    hacks.FreezeMana(manaSetValue);
                }
                else
                {
                    hacks.UnfreezeMana();
                }
            }
            
            ImGui.EndDisabled();
        }

        /// <summary>
        /// Renders the Inventory tab with coin management and item controls
        /// </summary>
        private void RenderInventoryTab()
        {
            ImGui.Text("Inventory & Currency Management");
            ImGui.Separator();

            // Coin section
            ImGui.Text("Coin Management");
            
            if (hacks.IsAttached)
            {
                var (copper, silver, gold, platinum) = hacks.GetCoins();
                ImGui.Text($"Current: {copper}c, {silver}s, {gold}g, {platinum}p");
                
                // Update UI values with current game values
                copperCoins = copper;
                silverCoins = silver;
                goldCoins = gold;
                platinumCoins = platinum;
            }
            else
            {
                ImGui.TextDisabled("Current: Not connected");
            }

            ImGui.Columns(2, "CoinColumns");
            
            ImGui.InputInt("Copper", ref copperCoins);
            ImGui.InputInt("Silver", ref silverCoins);
            
            ImGui.NextColumn();
            
            ImGui.InputInt("Gold", ref goldCoins);
            ImGui.InputInt("Platinum", ref platinumCoins);
            
            ImGui.Columns(1);
            
            ImGui.BeginDisabled(!hacks.IsAttached);
            
            if (ImGui.Button("Set Coins"))
            {
                hacks.SetCoins(copperCoins, silverCoins, goldCoins, platinumCoins);
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Give Max Money"))
            {
                hacks.GiveMaxMoney();
            }
            
            ImGui.EndDisabled();
            
            ImGui.Spacing();
            
            // Item section placeholder
            ImGui.Text("Item Management");
            ImGui.TextDisabled("(Item manipulation features would go here)");
            ImGui.TextDisabled("Find item IDs using Cheat Engine and update TerrariaItems class");
        }

        /// <summary>
        /// Renders the World tab with time, weather, and world state controls
        /// </summary>
        private void RenderWorldTab()
        {
            ImGui.Text("World State & Time Management");
            ImGui.Separator();

            // Time controls
            ImGui.Text("Time of Day");
            
            if (hacks.IsAttached)
            {
                float currentTime = hacks.GetGameTime();
                if (currentTime >= 0)
                {
                    string timeDesc = currentTime switch
                    {
                        < 0.25f => "Dawn",
                        < 0.5f => "Morning", 
                        < 0.75f => "Noon",
                        _ => "Night"
                    };
                    ImGui.Text($"Current Time: {currentTime:F2} ({timeDesc})");
                    gameTimeValue = currentTime;
                }
                else
                {
                    ImGui.TextDisabled("Current Time: Unable to read");
                }
            }
            else
            {
                ImGui.TextDisabled("Current Time: Not connected");
            }

            // Time slider with labeled positions
            ImGui.SliderFloat("Set Time", ref gameTimeValue, 0.0f, 1.0f);
            
            // Time preset buttons
            ImGui.Text("Time Presets:");
            
            ImGui.BeginDisabled(!hacks.IsAttached);
            
            if (ImGui.Button("Dawn (0.0)"))
            {
                gameTimeValue = 0.0f;
                hacks.SetGameTime(gameTimeValue);
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Noon (0.5)"))
            {
                gameTimeValue = 0.5f;
                hacks.SetGameTime(gameTimeValue);
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Night (1.0)"))
            {
                gameTimeValue = 1.0f;
                hacks.SetGameTime(gameTimeValue);
            }
            
            if (ImGui.Button("Set Custom Time"))
            {
                hacks.SetGameTime(gameTimeValue);
            }
            
            // Time freeze checkbox
            bool timeFrozen = hacks.IsTimeFrozen();
            if (ImGui.Checkbox("Freeze Time", ref timeFrozen))
            {
                if (timeFrozen)
                {
                    hacks.FreezeTime();
                }
                else
                {
                    hacks.UnfreezeTime();
                }
            }
            
            ImGui.EndDisabled();
            
            ImGui.Spacing();
            
            // Weather section placeholder
            ImGui.Text("Weather Controls");
            ImGui.TextDisabled("(Weather manipulation features would go here)");
            ImGui.TextDisabled("Find weather addresses using Cheat Engine");
        }

        /// <summary>
        /// Renders the Teleport tab with position controls and teleportation features
        /// </summary>
        private void RenderTeleportTab()
        {
            ImGui.Text("Player Position & Teleportation");
            ImGui.Separator();

            // Current position display
            if (hacks.IsAttached)
            {
                var (x, y) = hacks.CurrentPosition;
                ImGui.Text($"Current Position: ({x:F1}, {y:F1})");
                
                // Update teleport values with current position
                if (ImGui.Button("Use Current Position"))
                {
                    teleportX = x;
                    teleportY = y;
                }
            }
            else
            {
                ImGui.TextDisabled("Current Position: Not connected");
            }

            ImGui.Spacing();

            // Teleport controls
            ImGui.Text("Teleport Coordinates");
            ImGui.InputFloat("X Position", ref teleportX);
            ImGui.InputFloat("Y Position", ref teleportY);
            
            ImGui.BeginDisabled(!hacks.IsAttached);
            
            if (ImGui.Button("Teleport to Position"))
            {
                hacks.TeleportTo(teleportX, teleportY);
            }
            
            ImGui.Spacing();
            
            // Position freeze
            bool positionFrozen = hacks.IsPositionFrozen();
            if (ImGui.Checkbox("Freeze Position", ref positionFrozen))
            {
                if (positionFrozen)
                {
                    hacks.FreezePosition();
                }
                else
                {
                    hacks.UnfreezePosition();
                }
            }
            
            ImGui.EndDisabled();
            
            ImGui.Spacing();
            
            // Teleport presets (placeholder for common locations)
            ImGui.Text("Quick Teleport Presets");
            ImGui.TextDisabled("(Add your favorite locations here)");
            
            // Example preset buttons
            ImGui.BeginDisabled(!hacks.IsAttached);
            
            if (ImGui.Button("Spawn Point"))
            {
                // Default spawn coordinates - these would need to be determined for each world
                hacks.TeleportTo(0f, 0f);
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Underground"))
            {
                // Example underground coordinates
                hacks.TeleportTo(0f, 500f);
            }
            
            ImGui.EndDisabled();
        }

        /// <summary>
        /// Renders the Settings tab with UI customization and utility functions
        /// </summary>
        private void RenderSettingsTab()
        {
            ImGui.Text("Settings & Utilities");
            ImGui.Separator();

            // General settings
            ImGui.Text("General");
            
            if (ImGui.Button("Disable All Hacks"))
            {
                hacks.DisableAllHacks();
            }
            
            ImGui.Spacing();
            
            // UI settings
            ImGui.Text("Interface");
            
            if (ImGui.Button("About"))
            {
                showAboutWindow = true;
            }
            
            ImGui.Spacing();
            
            // Memory addresses info
            ImGui.Text("Memory Addresses");
            ImGui.TextDisabled("Make sure to update TerrariaAddresses.cs with");
            ImGui.TextDisabled("addresses found using Cheat Engine!");
            
            ImGui.Spacing();
            
            // Debug information
            if (ImGui.CollapsingHeader("Debug Information"))
            {
                ImGui.Text($"Attached: {hacks.IsAttached}");
                
                if (hacks.IsAttached)
                {
                    ImGui.Text("Memory values (current session):");
                    var (health, maxHealth) = hacks.CurrentHealth;
                    var (mana, maxMana) = hacks.CurrentMana;
                    var (x, y) = hacks.CurrentPosition;
                    
                    ImGui.Text($"Health: {health}/{maxHealth}");
                    ImGui.Text($"Mana: {mana}/{maxMana}"); 
                    ImGui.Text($"Position: ({x:F1}, {y:F1})");
                    
                    float time = hacks.GetGameTime();
                    ImGui.Text($"Game Time: {time:F3}");
                }
            }
        }

        /// <summary>
        /// Renders the About window with information and instructions
        /// </summary>
        private void RenderAboutWindow()
        {
            if (ImGui.Begin("About Terraria Tweaker", ref showAboutWindow))
            {
                ImGui.Text("Terraria Tweaker - Game Hack Menu");
                ImGui.Separator();
                
                ImGui.Text("A modern, feature-rich hack menu for Terraria");
                ImGui.Text("built with C# and ImGui.NET");
                
                ImGui.Spacing();
                
                ImGui.Text("How to use:");
                ImGui.BulletText("Start Terraria and load into a world");
                ImGui.BulletText("Open Cheat Engine and attach to Terraria.exe");
                ImGui.BulletText("Find memory addresses for the values you want to modify");
                ImGui.BulletText("Update the TerrariaAddresses.cs file with your findings");
                ImGui.BulletText("Run this program as Administrator");
                ImGui.BulletText("The program will automatically detect and attach to Terraria");
                
                ImGui.Spacing();
                
                ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), "Important Notes:");
                ImGui.BulletText("This tool is for educational purposes and single-player use only");
                ImGui.BulletText("Memory addresses change between game updates");
                ImGui.BulletText("Always backup your save files before using any cheats");
                ImGui.BulletText("Use responsibly and respect other players in multiplayer");
                
                ImGui.Spacing();
                
                if (ImGui.Button("Close"))
                {
                    showAboutWindow = false;
                }
            }
            ImGui.End();
        }
    }
}