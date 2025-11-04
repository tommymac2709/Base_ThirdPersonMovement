# Stats UI Setup Guide

## Overview
The StatsUIController provides automatic UI updates for Health, Stamina, Mana, Experience, and Level from the PlayerStatsModule.

## Creating the UI in Unity

### 1. Create Canvas
1. Right-click in Hierarchy → UI → Canvas
2. Set Canvas Scaler to "Scale With Screen Size" (1920x1080 reference)

### 2. Create Stats Panel
1. Right-click Canvas → UI → Panel
2. Rename to "StatsPanel"
3. Anchor to bottom-left corner

### 3. Create Health Bar
1. Right-click StatsPanel → UI → Slider
2. Rename to "HealthSlider"
3. Set colors: Background (dark red), Fill (bright red)
4. Add TextMeshProUGUI as child of slider for "HP: 100/100"

### 4. Create Stamina Bar
1. Duplicate HealthSlider
2. Rename to "StaminaSlider"
3. Set colors: Background (dark yellow), Fill (bright yellow)
4. Update text to show stamina

### 5. Create Mana Bar
1. Duplicate HealthSlider
2. Rename to "ManaSlider"
3. Set colors: Background (dark blue), Fill (bright blue)
4. Update text to show mana

### 6. Create Level Display
1. Right-click StatsPanel → UI → TextMeshProUGUI
2. Rename to "LevelText"
3. Set text to "Level 1"
4. Font size: 24, Bold

### 7. Create XP Bar
1. Right-click StatsPanel → UI → Slider
2. Rename to "XPSlider"
3. Set colors: Background (dark gray), Fill (bright green)
4. Add TextMeshProUGUI for "0/100 XP"

### 8. Add StatsUIController Component
1. Select StatsPanel
2. Add Component → StatsUIController
3. Drag UI elements to corresponding fields:
   - Health Slider → HealthSlider
   - Health Text → HealthSlider's text child
   - (repeat for stamina, mana, xp)
   - Level Text → LevelText
   - Player State Machine → Drag player from scene (or leave empty for auto-find)

### 9. Save as Prefab
1. Drag StatsPanel to Project window
2. Save as "StatsDisplayUI.prefab"

## Using the UI

The StatsUIController automatically:
- Finds the PlayerStateMachine (or use the field to assign manually)
- Gets the PlayerStatsModule
- Subscribes to all stat change events
- Updates UI in real-time

No additional code needed!

## Testing

To test the UI:
1. Add PlayerStatsModule to your player's modules list
2. Configure health/stamina/mana values in the module
3. Run the game
4. Use the console to test:
   ```csharp
   var stats = playerStateMachine.GetModule<PlayerStatsModule>();
   stats.DealDamage(25f);           // Test health
   stats.UseStamina(30f);           // Test stamina
   stats.UseMana(20f);              // Test mana
   stats.GainExperience(100f);      // Test XP/leveling
   ```

## Optional Enhancements

Consider adding:
- Animated health/stamina loss (lerp to new values)
- Flash effect when taking damage
- Level up particle effect
- Audio feedback for low health/stamina
- Floating damage numbers
