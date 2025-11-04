# Player Stats Modules API Reference

## Overview

The player stats system is composed of **4 independent, modular ScriptableObjects**:

1. **PlayerProgressionModule** - Experience tracking, leveling, stat scaling
2. **PlayerHealthModule** - Health, damage, death, regeneration
3. **PlayerStaminaModule** - Stamina usage and regeneration
4. **PlayerManaModule** - Mana usage and regeneration

Each module is **fully self-contained** and can be used independently. They optionally integrate with each other for enhanced functionality (e.g., health/stamina/mana scale with player level from ProgressionModule).

---

## Quick Setup

### One-Click Installation (Recommended for Beginners)

1. Select your player GameObject in the Hierarchy
2. Go to **MiST → Quick Setup → Add All Stats Modules**
3. Select which modules you want (Health, Stamina, Mana, Progression)
4. Click **Install Selected Modules**

This creates the module assets and adds them to your PlayerStateMachine automatically.

### Manual Installation (Advanced Users)

1. **Create Module Assets**:
   - Right-click in Project → Create → MiST → Player Modules → Progression
   - Right-click in Project → Create → MiST → Player Modules → Health
   - Right-click in Project → Create → MiST → Player Modules → Stamina
   - Right-click in Project → Create → MiST → Player Modules → Mana

2. **Add to Player**:
   - Select your player GameObject
   - Find PlayerStateMachine component
   - Add the created modules to the "Modules" list

3. **Configure Values** in Inspector (see Configuration sections below)

---

## 1. PlayerProgressionModule

Provides **experience tracking**, **leveling**, and **stat scaling multipliers** for other modules.

### Configuration

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `startingLevel` | int | 1 | Starting player level |
| `xpRequiredCurve` | AnimationCurve | Linear | XP required per level (X=level, Y=XP) |
| `statIncreasePerLevel` | float | 0.1 | % increase to max stats per level (0.1 = 10%) |
| `restoreResourcesOnLevelUp` | bool | true | Fully restore HP/Stamina/Mana on level up |

### Getting the Module

```csharp
PlayerProgressionModule progression = playerStateMachine.GetModule<PlayerProgressionModule>();
if (progression == null)
{
    Debug.LogError("PlayerProgressionModule not found!");
    return;
}
```

### Properties

```csharp
int CurrentLevel { get; }              // Current player level
float CurrentXP { get; }               // Current XP amount
float ExperienceToNextLevel { get; }   // XP required for next level
bool RestoreResourcesOnLevelUp { get; } // Config value (for other modules)
```

### Methods

```csharp
void GainExperience(float amount)      // Award XP (auto-levels if threshold reached)
float GetStatMultiplier()              // Returns stat multiplier (1.0 at level 1, increases with level)
```

### Events

```csharp
event Action OnLevelUp;                    // Fired when player levels up
event Action<float> OnExperienceGained;    // Fired when XP awarded (passes amount)
```

### Example Usage

```csharp
// Award XP
progression.GainExperience(100f);

// Subscribe to level up
progression.OnLevelUp += () => {
    Debug.Log($"Level up! Now level {progression.CurrentLevel}");
    ShowLevelUpEffect();
    PlayLevelUpSound();
};

// Track XP progress
progression.OnExperienceGained += (amount) => {
    float progress = progression.CurrentXP / progression.ExperienceToNextLevel;
    UpdateXPBar(progress);
};
```

### EventManager Integration

| Event Name | Parameter | Description |
|------------|-----------|-------------|
| `"Player.ExperienceGained"` | float | XP amount gained |
| `"Player.LevelUp"` | int | New level |

---

## 2. PlayerHealthModule

Provides **health system** with damage, healing, death, and auto-regeneration.

### Configuration

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `baseMaxHealth` | float | 100 | Maximum health at level 1 |
| `healthRegenRate` | float | 5 | HP regenerated per second |
| `healthRegenDelay` | float | 5 | Seconds after damage before regen starts |

> **Note**: MaxHealth automatically scales with player level if PlayerProgressionModule is present.

### Getting the Module

```csharp
PlayerHealthModule health = playerStateMachine.GetModule<PlayerHealthModule>();
if (health == null)
{
    Debug.LogError("PlayerHealthModule not found!");
    return;
}
```

### Properties

```csharp
float CurrentHealth { get; }      // Current HP
float MaxHealth { get; }          // Max HP (scales with level if ProgressionModule present)
bool IsDead { get; }              // Is player dead?
```

### Methods

```csharp
void TakeDamage(float damage)     // Deal damage to player
void Heal(float amount)           // Heal player by amount
void RestoreHealth()              // Fully restore health to max
void Revive(float healthAmount)   // Revive player (0 = full health)
```

### Events

```csharp
event Action<float> OnHealthChanged;  // Fired when HP changes (passes current HP)
event Action OnDeath;                 // Fired when player dies
```

### Example Usage

```csharp
// Deal damage
health.TakeDamage(25f);

// Heal
health.Heal(50f);

// Subscribe to events
health.OnHealthChanged += (currentHP) => {
    Debug.Log($"Health: {currentHP}/{health.MaxHealth}");
    UpdateHealthBar(currentHP, health.MaxHealth);
};

health.OnDeath += () => {
    Debug.Log("Player died!");
    ShowDeathScreen();
};

// Check if dead
if (health.IsDead)
{
    health.Revive(100f); // Revive with 100 HP
}
```

### EventManager Integration

| Event Name | Parameter | Description |
|------------|-----------|-------------|
| `"Player.HealthChanged"` | float | Current health |
| `"Player.Death"` | none | Player died |

---

## 3. PlayerStaminaModule

Provides **stamina system** for actions like sprinting, dodging, and blocking.

### Configuration

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `baseMaxStamina` | float | 100 | Maximum stamina at level 1 |
| `staminaRegenRate` | float | 20 | Stamina regenerated per second |
| `staminaRegenDelay` | float | 2 | Seconds after use before regen starts |

> **Note**: MaxStamina automatically scales with player level if PlayerProgressionModule is present.

### Getting the Module

```csharp
PlayerStaminaModule stamina = playerStateMachine.GetModule<PlayerStaminaModule>();
if (stamina == null)
{
    Debug.LogError("PlayerStaminaModule not found!");
    return;
}
```

### Properties

```csharp
float CurrentStamina { get; }     // Current stamina
float MaxStamina { get; }         // Max stamina (scales with level if ProgressionModule present)
```

### Methods

```csharp
bool Use(float amount)            // Returns false if insufficient
void Restore(float amount)        // Restore stamina (0 = restore to max)
```

### Events

```csharp
event Action<float> OnStaminaChanged;  // Fired when stamina changes
event Action OnStaminaDepleted;        // Fired when stamina reaches 0
```

### Example Usage

```csharp
// Check and use stamina
if (stamina.Use(20f))
{
    // Stamina used successfully, perform action
    Dodge();
}
else
{
    // Not enough stamina
    ShowLowStaminaWarning();
}

// Subscribe to depletion
stamina.OnStaminaDepleted += () => {
    StopSprinting();
    Debug.Log("Out of stamina!");
};

// Restore stamina (e.g., stamina potion)
stamina.Restore(50f);
```

### EventManager Integration

| Event Name | Parameter | Description |
|------------|-----------|-------------|
| `"Player.StaminaDepleted"` | none | Stamina reached 0 |

---

## 4. PlayerManaModule

Provides **mana system** for spells and abilities.

### Configuration

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `baseMaxMana` | float | 100 | Maximum mana at level 1 |
| `manaRegenRate` | float | 10 | Mana regenerated per second |
| `manaRegenDelay` | float | 3 | Seconds after use before regen starts |

> **Note**: MaxMana automatically scales with player level if PlayerProgressionModule is present.

### Getting the Module

```csharp
PlayerManaModule mana = playerStateMachine.GetModule<PlayerManaModule>();
if (mana == null)
{
    Debug.LogError("PlayerManaModule not found!");
    return;
}
```

### Properties

```csharp
float CurrentMana { get; }        // Current mana
float MaxMana { get; }            // Max mana (scales with level if ProgressionModule present)
```

### Methods

```csharp
bool Use(float amount)            // Returns false if insufficient
void Restore(float amount)        // Restore mana (0 = restore to max)
```

### Events

```csharp
event Action<float> OnManaChanged;  // Fired when mana changes
```

### Example Usage

```csharp
// Cast spell
if (mana.Use(30f))
{
    CastFireball();
}
else
{
    ShowLowManaWarning();
}

// Mana potion
mana.Restore(50f);

// Subscribe to changes
mana.OnManaChanged += (currentMana) => {
    UpdateManaBar(currentMana, mana.MaxMana);
};
```

### EventManager Integration

| Event Name | Parameter | Description |
|------------|-----------|-------------|
| `"Player.ManaChanged"` | float | Current mana |

---

## Module Integration

### How Modules Work Together

Modules can **optionally integrate** with each other without hard dependencies:

```csharp
// PlayerHealthModule checks for PlayerProgressionModule
public float MaxHealth
{
    get
    {
        var progression = cachedStateMachine?.GetModule<PlayerProgressionModule>();
        float multiplier = progression?.GetStatMultiplier() ?? 1f;
        return baseMaxHealth * multiplier;
    }
}
```

**Benefits**:
- Use modules independently (e.g., Health-only without Progression)
- MaxHealth/MaxStamina/MaxMana automatically scale with level if ProgressionModule present
- Resources can auto-restore on level-up if `restoreResourcesOnLevelUp` is enabled

### Example: Level Scaling

```csharp
// At Level 1:
// - baseMaxHealth = 100, multiplier = 1.0 → MaxHealth = 100
// - baseMaxStamina = 100, multiplier = 1.0 → MaxStamina = 100

// At Level 5 (with statIncreasePerLevel = 0.1):
// - multiplier = 1.0 + (5-1) * 0.1 = 1.4
// - MaxHealth = 100 * 1.4 = 140
// - MaxStamina = 100 * 1.4 = 140
```

---

## Integration Examples

### Example 1: Sprint with Stamina Drain

```csharp
// In PlayerFreeMovementState
public override void Enter()
{
    // ... existing code ...

    var stamina = stateMachine.GetModule<PlayerStaminaModule>();
    if (stamina != null)
    {
        stateMachine.InputBridge.SprintStartEvent += () => StartSprint(stamina);
        stateMachine.InputBridge.SprintStopEvent += StopSprint;

        // Stop sprinting when stamina depleted
        stamina.OnStaminaDepleted += StopSprint;
    }
}

private void StartSprint(PlayerStaminaModule stamina)
{
    if (isSprinting) return;
    isSprinting = true;
    stateMachine.StartCoroutine(DrainStaminaWhileSprinting(stamina));
}

private IEnumerator DrainStaminaWhileSprinting(PlayerStaminaModule stamina)
{
    while (isSprinting)
    {
        if (!stamina.Use(10f * Time.deltaTime)) // 10 stamina/second
        {
            StopSprint();
            yield break;
        }
        yield return null;
    }
}
```

### Example 2: Health Pickup Item

```csharp
public class HealthPotion : MonoBehaviour
{
    [SerializeField] private float healAmount = 50f;

    private void OnTriggerEnter(Collider other)
    {
        var sm = other.GetComponent<PlayerStateMachine>();
        if (sm != null)
        {
            var health = sm.GetModule<PlayerHealthModule>();
            if (health != null)
            {
                health.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}
```

### Example 3: Enemy Dealing Damage

```csharp
public class Enemy : MonoBehaviour
{
    [SerializeField] private float damageAmount = 25f;

    private void OnCollisionEnter(Collision collision)
    {
        var sm = collision.gameObject.GetComponent<PlayerStateMachine>();
        if (sm != null)
        {
            var health = sm.GetModule<PlayerHealthModule>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
        }
    }
}
```

### Example 4: Ability Costing Mana

```csharp
public class FireballAbility
{
    private const float ManaCost = 30f;

    public bool TryCast(PlayerStateMachine sm)
    {
        var mana = sm.GetModule<PlayerManaModule>();
        if (mana == null) return false;

        if (mana.Use(ManaCost))
        {
            // Cast fireball
            SpawnFireball();
            return true;
        }

        Debug.Log("Not enough mana!");
        return false;
    }
}
```

### Example 5: XP Reward on Kill

```csharp
public class Enemy : MonoBehaviour
{
    [SerializeField] private float xpReward = 50f;

    public void Die(GameObject killer)
    {
        // Award XP to killer
        var sm = killer.GetComponent<PlayerStateMachine>();
        if (sm != null)
        {
            var progression = sm.GetModule<PlayerProgressionModule>();
            if (progression != null)
            {
                progression.GainExperience(xpReward);
            }
        }

        Destroy(gameObject);
    }
}
```

### Example 6: Dodge Ability Costing Stamina

```csharp
public class DodgeAbility
{
    private const float StaminaCost = 25f;

    public bool TryDodge(PlayerStateMachine sm)
    {
        var stamina = sm.GetModule<PlayerStaminaModule>();
        if (stamina == null) return false;

        if (stamina.Use(StaminaCost))
        {
            PerformDodgeRoll();
            return true;
        }

        Debug.Log("Not enough stamina!");
        return false;
    }
}
```

---

## EventManager Integration (Global Events)

All modules broadcast events globally via EventManager for cross-system communication:

```csharp
using MistInteractive.ThirdPerson;

// Listen to global events
EventManager.StartListening<float>("Player.HealthChanged", OnPlayerHealthChanged);
EventManager.StartListening("Player.Death", OnPlayerDeath);
EventManager.StartListening<int>("Player.LevelUp", OnPlayerLevelUp);

// Unsubscribe
EventManager.StopListening<float>("Player.HealthChanged", OnPlayerHealthChanged);
```

**All Available Events**:

| Event Name | Parameter | Source Module |
|------------|-----------|---------------|
| `"Player.HealthChanged"` | float | PlayerHealthModule |
| `"Player.Death"` | none | PlayerHealthModule |
| `"Player.StaminaDepleted"` | none | PlayerStaminaModule |
| `"Player.ManaChanged"` | float | PlayerManaModule |
| `"Player.ExperienceGained"` | float | PlayerProgressionModule |
| `"Player.LevelUp"` | int | PlayerProgressionModule |

---

## Troubleshooting

### Module not found
- Use **MiST → Quick Setup → Add All Stats Modules** for automatic setup
- If manual: Check that module assets are in the Modules list
- Verify the assets are not null in Inspector

### Events not firing
- Make sure you subscribed correctly: `health.OnHealthChanged += Method;`
- Check that module was installed (happens in Start())
- Verify you didn't subscribe too early (before Start())

### Stats not regenerating
- Check regen delays in module configuration
- Make sure coroutines are running (module needs PlayerStateMachine)
- Verify regeneration isn't interrupted (using resource resets delay)

### Level not increasing
- Check xpRequiredCurve in Inspector
- Verify curve has values for your target levels
- Use Debug.Log to check CurrentXP vs ExperienceToNextLevel

### Max stats not scaling with level
- Ensure PlayerProgressionModule is installed alongside Health/Stamina/Mana modules
- Check `statIncreasePerLevel` value in PlayerProgressionModule
- Verify the module is in the same PlayerStateMachine

---

## Architecture: Why Separate Modules?

### Single Responsibility Principle
Each module has **one clear purpose**:
- **PlayerProgressionModule**: Level progression
- **PlayerHealthModule**: Health management
- **PlayerStaminaModule**: Stamina management
- **PlayerManaModule**: Mana management

### Benefits
1. **Modularity**: Use only what you need (e.g., Health-only game)
2. **Maintainability**: Each module is ~100-150 lines, easy to understand
3. **Extensibility**: Add new modules without modifying existing ones
4. **Testability**: Test each system independently
5. **Asset Store Strategy**: Sell as separate packs or bundles

---

## Future Expansion

These modules integrate seamlessly with future modules:

**CombatModule** will use:
- `health.TakeDamage()` for weapon hits
- `stamina.Use()` for attacks/blocks/dodges

**AbilityModule** will use:
- `mana.Use()` for casting spells
- `progression.CurrentLevel` for ability unlocks

**EquipmentModule** will use:
- `progression.OnLevelUp` to check equip requirements
- Stat modifiers (future enhancement)

**QuestModule** will use:
- `progression.GainExperience()` for quest rewards
- Level checks for quest requirements
