# PlayerStatsModule API Reference

## Overview

The **PlayerStatsModule** is a self-contained ScriptableObject-based module that provides:
- **Health System** - Damage, healing, death, regeneration
- **Stamina System** - Usage, regeneration, depletion
- **Mana System** - Usage, regeneration
- **Progression System** - Experience tracking, level-ups

## Installation

### 1. Create the Module Asset
1. Right-click in Project → Create → MiST → Player Modules → Player Stats
2. Name it (e.g., "PlayerStatsModule_Default")
3. Configure values in Inspector

### 2. Add to Player
1. Select your player GameObject
2. Find PlayerStateMachine component
3. Add the module to the "Modules" list

### 3. Configure Values
See [Configuration](#configuration) section below.

---

## Configuration

### Health System
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `baseMaxHealth` | float | 100 | Maximum health at level 1 |
| `healthRegenRate` | float | 5 | HP regenerated per second |
| `healthRegenDelay` | float | 5 | Seconds after damage before regen starts |

### Stamina System
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `baseMaxStamina` | float | 100 | Maximum stamina at level 1 |
| `staminaRegenRate` | float | 20 | Stamina regenerated per second |
| `staminaRegenDelay` | float | 2 | Seconds after use before regen starts |

### Mana System
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `baseMaxMana` | float | 100 | Maximum mana at level 1 |
| `manaRegenRate` | float | 10 | Mana regenerated per second |
| `manaRegenDelay` | float | 3 | Seconds after use before regen starts |

### Progression System
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `startingLevel` | int | 1 | Starting player level |
| `xpRequiredCurve` | AnimationCurve | Linear | XP required per level (X=level, Y=XP) |
| `regenerateOnLevelUp` | bool | true | Fully restore resources on level up |
| `statIncreasePerLevel` | float | 0.1 | % increase to max stats per level (0.1 = 10%) |

---

## Public API

### Getting the Module

```csharp
PlayerStatsModule stats = playerStateMachine.GetModule<PlayerStatsModule>();
if (stats == null)
{
    Debug.LogError("PlayerStatsModule not found!");
    return;
}
```

### Health System

#### Properties
```csharp
float CurrentHealth { get; }      // Current HP
float MaxHealth { get; }          // Max HP (increases with level)
bool IsDead { get; }              // Is player dead?
```

#### Methods
```csharp
void DealDamage(float damage)     // Deal damage to player
void Heal(float amount)           // Heal player
void RestoreHealth()              // Fully restore health
```

#### Events
```csharp
event Action<float> OnHealthChanged;  // Fired when HP changes (passes current HP)
event Action OnDeath;                 // Fired when player dies
```

#### Example Usage
```csharp
// Deal damage
stats.DealDamage(25f);

// Heal
stats.Heal(50f);

// Subscribe to events
stats.OnHealthChanged += (currentHP) => {
    Debug.Log($"Health: {currentHP}/{stats.MaxHealth}");
    UpdateHealthBar(currentHP, stats.MaxHealth);
};

stats.OnDeath += () => {
    Debug.Log("Player died!");
    ShowDeathScreen();
};
```

### Stamina System

#### Properties
```csharp
float CurrentStamina { get; }     // Current stamina
float MaxStamina { get; }         // Max stamina (increases with level)
```

#### Methods
```csharp
bool UseStamina(float amount)     // Returns false if insufficient
void RestoreStamina(float amount) // Restore stamina
```

#### Events
```csharp
event Action<float> OnStaminaChanged;  // Fired when stamina changes
event Action OnStaminaDepleted;        // Fired when stamina reaches 0
```

#### Example Usage
```csharp
// Check and use stamina
if (stats.UseStamina(20f))
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
stats.OnStaminaDepleted += () => {
    StopSprinting();
    Debug.Log("Out of stamina!");
};
```

### Mana System

#### Properties
```csharp
float CurrentMana { get; }        // Current mana
float MaxMana { get; }            // Max mana (increases with level)
```

#### Methods
```csharp
bool UseMana(float amount)        // Returns false if insufficient
void RestoreMana(float amount)    // Restore mana
```

#### Events
```csharp
event Action<float> OnManaChanged;  // Fired when mana changes
```

#### Example Usage
```csharp
// Cast spell
if (stats.UseMana(30f))
{
    CastFireball();
}
else
{
    ShowLowManaWarning();
}

// Mana potion
stats.RestoreMana(50f);
```

### Progression System

#### Properties
```csharp
int CurrentLevel { get; }              // Current player level
float CurrentXP { get; }               // Current XP amount
float ExperienceToNextLevel { get; }   // XP required for next level
```

#### Methods
```csharp
void GainExperience(float amount)      // Award XP (auto-levels if threshold reached)
```

#### Events
```csharp
event Action OnLevelUp;                    // Fired when player levels up
event Action<float> OnExperienceGained;    // Fired when XP awarded (passes amount)
```

#### Example Usage
```csharp
// Award XP
stats.GainExperience(100f);

// Subscribe to level up
stats.OnLevelUp += () => {
    Debug.Log($"Level up! Now level {stats.CurrentLevel}");
    ShowLevelUpEffect();
    PlayLevelUpSound();
};

// Track XP progress
stats.OnExperienceGained += (amount) => {
    float progress = stats.CurrentXP / stats.ExperienceToNextLevel;
    UpdateXPBar(progress);
};
```

---

## EventManager Integration

The module also broadcasts events globally via EventManager for cross-system communication:

### Global Event Names

| Event Name | Parameter Type | Description |
|------------|----------------|-------------|
| `"Player.HealthChanged"` | float | Current health |
| `"Player.Death"` | none | Player died |
| `"Player.StaminaDepleted"` | none | Stamina reached 0 |
| `"Player.ManaChanged"` | float | Current mana |
| `"Player.ExperienceGained"` | float | XP amount gained |
| `"Player.LevelUp"` | int | New level |

### Example Usage

```csharp
using MistInteractive.ThirdPerson;

// Listen to global events
EventManager.StartListening<float>("Player.HealthChanged", OnPlayerHealthChanged);
EventManager.StartListening("Player.Death", OnPlayerDeath);
EventManager.StartListening<int>("Player.LevelUp", OnPlayerLevelUp);

// Unsubscribe
EventManager.StopListening<float>("Player.HealthChanged", OnPlayerHealthChanged);
```

---

## Integration Examples

### Example 1: Sprint with Stamina Drain

```csharp
// In PlayerFreeMovementState
public override void Enter()
{
    // ... existing code ...

    var stats = stateMachine.GetModule<PlayerStatsModule>();
    if (stats != null)
    {
        stateMachine.InputBridge.SprintStartEvent += () => StartSprint(stats);
        stateMachine.InputBridge.SprintStopEvent += StopSprint;

        // Stop sprinting when stamina depleted
        stats.OnStaminaDepleted += StopSprint;
    }
}

private void StartSprint(PlayerStatsModule stats)
{
    if (isSprinting) return;
    isSprinting = true;
    stateMachine.StartCoroutine(DrainStaminaWhileSprinting(stats));
}

private IEnumerator DrainStaminaWhileSprinting(PlayerStatsModule stats)
{
    while (isSprinting)
    {
        if (!stats.UseStamina(10f * Time.deltaTime)) // 10 stamina/second
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
            var stats = sm.GetModule<PlayerStatsModule>();
            if (stats != null)
            {
                stats.Heal(healAmount);
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
            var stats = sm.GetModule<PlayerStatsModule>();
            if (stats != null)
            {
                stats.DealDamage(damageAmount);
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
        var stats = sm.GetModule<PlayerStatsModule>();
        if (stats == null) return false;

        if (stats.UseMana(ManaCost))
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
            var stats = sm.GetModule<PlayerStatsModule>();
            if (stats != null)
            {
                stats.GainExperience(xpReward);
            }
        }

        Destroy(gameObject);
    }
}
```
---

## Troubleshooting

### Module not found
- Check that PlayerStatsModule asset is in the Modules list
- Verify the asset is not null in Inspector

### Events not firing
- Make sure you subscribed correctly: `stats.OnHealthChanged += Method;`
- Check that module was installed (happens in Start())
- Verify you didn't subscribe too early (before Start())

### Stats not regenerating
- Check regen delays in module configuration
- Make sure coroutines are running (module needs PlayerStateMachine)
- Verify regeneration isn't interrupted (taking damage resets delay)

### Level not increasing
- Check xpRequiredCurve in Inspector
- Verify curve has values for your target levels
- Use Debug.Log to check CurrentXP vs ExperienceToNextLevel

---

## Future Expansion

This module is designed to be extended by future modules:

**CombatModule** will use:
- `DealDamage()` for weapon hits
- `UseStamina()` for attacks/blocks

**AbilityModule** will use:
- `UseMana()` for casting spells
- Level requirements for ability unlocks

**EquipmentModule** will use:
- `OnLevelUp` to check for equip requirements
- Stat modifiers (future enhancement)

**QuestModule** will use:
- `GainExperience()` for quest rewards
- Level checks for quest requirements
