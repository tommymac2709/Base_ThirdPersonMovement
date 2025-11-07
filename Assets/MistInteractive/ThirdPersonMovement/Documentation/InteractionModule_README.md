# Interaction Module Documentation

## Overview

The **Interaction Module** is a flexible, modular system that enables your player to interact with objects in the game world. It follows the same modular architecture as the Player Stats system, working independently while providing a robust API for future modules (inventory, combat, etc.) to hook into.

### Key Features

- **Modular Design**: Works independently, can be omitted if not needed
- **Flexible Detection**: Sphere-based detection with forward-facing angle filtering
- **Type-Safe Events**: C# events for loose coupling with other systems
- **Self-Describing UI**: Each interactable defines its own visual presentation
- **Context-Sensitive**: Validation hooks allow conditional interactions
- **Single Button**: Simplified interaction using one input (E key by default)
- **Visual Feedback**: Gizmos for debugging detection range and angle
- **Designer-Friendly**: Includes UnityEvent-based interactables for non-programmers

---

## Installation

### Quick Setup (Recommended)

1. Open Unity menu: **MiST → Quick Setup → Add Interaction Module**
2. Select your PlayerStateMachine in the scene
3. Click **"Create InteractionModule Asset"** (if needed)
4. Click **"Add Interaction Module to PlayerStateMachine"**
5. Done! The module is now installed.

### Manual Setup

1. Create InteractionModule asset:
   - Right-click in Project → **Create → MiST → Player Modules → Interaction**
2. Add module to PlayerStateMachine:
   - Select your Player GameObject in the scene
   - Find the PlayerStateMachine component
   - Add the InteractionModule to the **Modules** list
   - Click **"Auto-Detect Modules in Project"** button (optional)

### Input Setup (Already Done)

The interact input (E key) is already configured in the Controls.inputactions file. If Unity doesn't auto-regenerate the Controls.cs file:
1. Select **Assets/MistInteractive/ThirdPersonMovement/Controls.inputactions**
2. Click **"Generate C# Class"** in the Inspector

---

## Quick Start

### Creating Your First Interactable

The easiest way to create an interactable is using the built-in examples:

#### 1. Simple Debug Interactable

```csharp
using MistInteractive.ThirdPerson.Interaction.Examples;

// Add to any GameObject
public class MyObject : SimpleInteractable
{
    // That's it! Will log a debug message when interacted with
}
```

#### 2. Event-Based Interactable (Designer-Friendly)

1. Add **EventInteractable** component to any GameObject
2. Set the **Prompt Text** (e.g., "Open Chest")
3. Add your logic to the **On Interact** UnityEvent in the Inspector
4. Done!

Example uses:
- Opening doors
- Triggering cutscenes
- Playing sounds
- Spawning objects

#### 3. Custom Interactable

```csharp
using UnityEngine;
using MistInteractive.ThirdPerson.Interaction;

public class MyCustomInteractable : InteractableBase
{
    public override void Interact(Transform interactor)
    {
        Debug.Log("Player interacted with " + gameObject.name);

        // Your custom logic here
        // Example: Open a door, pick up an item, start dialogue, etc.
    }
}
```

---

## Setting Up UI (Optional)

To display interaction prompts to the player:

### 1. Create UI Canvas

1. Create a Canvas: **GameObject → UI → Canvas**
2. Set **Render Mode** to **Screen Space - Overlay**

### 2. Create Interaction Prompt

1. Create a Panel as a child of Canvas (this will be the container)
2. Add TextMeshPro elements for:
   - Prompt text (e.g., "Open Chest")
   - Button text (e.g., "E")
3. Optionally add an Image for an icon

### 3. Add InteractionUIController

1. Create an empty GameObject (or use the Canvas itself)
2. Add **InteractionUIController** component
3. Assign the UI elements:
   - **Container**: The panel GameObject
   - **Prompt Text**: The TextMeshPro for the prompt
   - **Button Text**: The TextMeshPro for the key
   - **Icon Image**: (Optional) Image component for icons

The UI will automatically show/hide based on detected interactables!

---

## Creating Custom Interactables

### Method 1: Extend InteractableBase (Recommended)

**InteractableBase** provides default implementations and serialized fields:

```csharp
using UnityEngine;
using MistInteractive.ThirdPerson.Interaction;

public class Door : InteractableBase
{
    [Header("Door Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private Animation doorAnimation;

    public override void Interact(Transform interactor)
    {
        if (isLocked)
        {
            Debug.Log("Door is locked!");
            return;
        }

        // Open the door
        doorAnimation.Play("DoorOpen");
        SetEnabled(false); // Disable further interactions
    }

    public override bool CanInteract(Transform interactor)
    {
        // Red text if locked
        if (isLocked)
            return false;

        return base.CanInteract(interactor);
    }

    public override InteractionUIData GetUIData()
    {
        var data = base.GetUIData();

        // Change prompt based on state
        if (isLocked)
        {
            data.promptText = "Locked";
            data.promptColor = Color.red;
        }

        return data;
    }

    public void Unlock()
    {
        isLocked = false;
        SetPromptText("Open Door");
    }
}
```

### Method 2: Implement IInteractable Directly

For maximum flexibility, implement the interface from scratch:

```csharp
using UnityEngine;
using MistInteractive.ThirdPerson.Interaction;

public class ComplexInteractable : MonoBehaviour, IInteractable
{
    public void Interact(Transform interactor)
    {
        // Your logic
    }

    public InteractionUIData GetUIData()
    {
        return new InteractionUIData
        {
            promptText = "Custom Prompt",
            buttonText = "E",
            promptColor = Color.white,
            icon = null,
            enabled = true
        };
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool CanInteract(Transform interactor)
    {
        return true; // Your validation logic
    }
}
```

---

## Public API Reference

### PlayerInteractionModule

Access via: `playerStateMachine.GetModule<PlayerInteractionModule>()`

#### Properties

```csharp
float InteractionRange { get; set; }          // Get/set detection range
float DetectionAngle { get; set; }            // Get/set detection angle (0-180)
string InteractKeyName { get; }               // Get default key name for UI
bool IsInteractionEnabled { get; }            // Check if interactions are enabled
```

#### Methods

```csharp
IInteractable GetCurrentInteractable()        // Get currently detected interactable
bool TryInteract()                            // Attempt interaction with current target
void SetInteractionEnabled(bool enabled)      // Enable/disable interactions
void SetDetectionActive(bool active)          // Enable/disable detection entirely
```

#### Events

```csharp
event Action<IInteractable> OnInteractableDetected   // Fired when new interactable is focused
event Action OnInteractableLost                      // Fired when interactable is lost
event Action<IInteractable, Transform> OnInteractionPerformed  // Fired after interaction
```

### IInteractable Interface

```csharp
void Interact(Transform interactor)              // Called when player interacts
InteractionUIData GetUIData()                    // Returns UI configuration
Transform GetTransform()                         // Returns this interactable's Transform
bool CanInteract(Transform interactor)           // Validates if interaction can occur
```

### InteractableBase Class

Extends IInteractable with default implementations and helper methods:

```csharp
void SetEnabled(bool value)                     // Enable/disable this interactable
void SetPromptText(string text)                 // Change prompt text dynamically
```

---

## Integration with Future Modules

The Interaction Module is designed to integrate seamlessly with future modules like Inventory and Combat.

### Example: Pickup with Inventory Check

```csharp
using UnityEngine;
using MistInteractive.ThirdPerson.Interaction;
using MistInteractive.ThirdPerson.Player;

public class ItemPickup : InteractableBase
{
    [SerializeField] private string itemID = "health_potion";

    public override void Interact(Transform interactor)
    {
        var stateMachine = interactor.GetComponent<PlayerStateMachine>();
        if (stateMachine == null) return;

        // Check for inventory module (future)
        var inventory = stateMachine.GetModule<InventoryModule>();
        if (inventory != null)
        {
            bool success = inventory.AddItem(itemID);
            if (success)
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory full!");
            }
        }
        else
        {
            // No inventory module - just destroy for now
            Debug.Log($"Picked up {itemID} (no inventory system)");
            Destroy(gameObject);
        }
    }

    public override bool CanInteract(Transform interactor)
    {
        if (!base.CanInteract(interactor)) return false;

        // Check inventory space (future)
        var stateMachine = interactor.GetComponent<PlayerStateMachine>();
        var inventory = stateMachine?.GetModule<InventoryModule>();

        if (inventory != null && !inventory.HasSpace())
        {
            return false; // Will show red/disabled UI
        }

        return true;
    }

    public override InteractionUIData GetUIData()
    {
        var data = base.GetUIData();

        // Visual feedback if inventory full (future)
        var playerSM = FindFirstObjectByType<PlayerStateMachine>();
        var inventory = playerSM?.GetModule<InventoryModule>();

        if (inventory != null && !inventory.HasSpace())
        {
            data.promptText = "Inventory Full";
            data.promptColor = Color.red;
        }

        return data;
    }
}
```

### Example: Weapon Pickup with Combat Module

```csharp
public class WeaponPickup : InteractableBase
{
    [SerializeField] private WeaponData weaponData;

    public override void Interact(Transform interactor)
    {
        var stateMachine = interactor.GetComponent<PlayerStateMachine>();
        var combatModule = stateMachine?.GetModule<CombatModule>();

        if (combatModule != null)
        {
            combatModule.EquipWeapon(weaponData);
            Destroy(gameObject);
        }
    }
}
```

### Subscribing to Interaction Events

Other systems can listen to interaction events:

```csharp
public class InteractionTracker : MonoBehaviour
{
    private PlayerInteractionModule interactionModule;

    private void Start()
    {
        var playerSM = FindFirstObjectByType<PlayerStateMachine>();
        interactionModule = playerSM?.GetModule<PlayerInteractionModule>();

        if (interactionModule != null)
        {
            interactionModule.OnInteractionPerformed += OnPlayerInteracted;
        }
    }

    private void OnDestroy()
    {
        if (interactionModule != null)
        {
            interactionModule.OnInteractionPerformed -= OnPlayerInteracted;
        }
    }

    private void OnPlayerInteracted(IInteractable interactable, Transform player)
    {
        Debug.Log($"Player interacted with: {interactable.GetTransform().name}");

        // Track stats, trigger achievements, etc.
    }
}
```

---

## Detection System

### How Detection Works

1. **Sphere Overlap**: Every frame, a sphere overlap check is performed at the player's position
2. **Forward Angle Filter**: Objects outside the forward-facing cone are excluded (default: 90 degrees)
3. **Interface Check**: Only objects with IInteractable component are considered
4. **Active Check**: Disabled GameObjects are automatically excluded
5. **Closest Selection**: The closest valid interactable is selected
6. **Event Notification**: Module fires events when focused interactable changes

### Configuring Detection

Settings are exposed in the InteractionModule asset:

- **Interaction Range**: Maximum detection distance (default: 3 units)
- **Detection Angle**: Forward-facing cone angle (default: 90 degrees)
- **Interactable Layers**: Layer mask for filtering
- **Interact Key Name**: The key name shown in UI (default: "E")

### Visual Debugging

Select any GameObject with an InteractionDetector component to see:
- **Yellow Wireframe Sphere**: Detection range
- **Cyan Lines**: Detection angle cone boundaries
- **Green Line**: Connection to currently detected interactable

---

## Best Practices

### 1. Use CanInteract() for Validation

Don't check conditions in Interact() - use CanInteract() instead:

```csharp
// ❌ Bad
public override void Interact(Transform interactor)
{
    if (!hasKey)
    {
        Debug.Log("You need a key!");
        return;
    }
    OpenDoor();
}

// ✅ Good
public override bool CanInteract(Transform interactor)
{
    return hasKey && base.CanInteract(interactor);
}

public override void Interact(Transform interactor)
{
    OpenDoor(); // Only called if CanInteract() returned true
}
```

### 2. Update UI Dynamically

Use GetUIData() to provide real-time feedback:

```csharp
public override InteractionUIData GetUIData()
{
    var data = base.GetUIData();

    // Dynamic prompt based on state
    if (isOpen)
    {
        data.promptText = "Close Door";
    }
    else if (isLocked)
    {
        data.promptText = "Locked (Need Key)";
        data.promptColor = Color.red;
    }
    else
    {
        data.promptText = "Open Door";
    }

    return data;
}
```

### 3. Graceful Module Degradation

Always check for null when accessing modules:

```csharp
var inventory = stateMachine?.GetModule<InventoryModule>();
if (inventory != null)
{
    // Use inventory
}
else
{
    // Fallback behavior (works without inventory)
}
```

### 4. Unsubscribe from Events

Always unsubscribe in OnDestroy() to prevent memory leaks:

```csharp
private void OnDestroy()
{
    if (interactionModule != null)
    {
        interactionModule.OnInteractionPerformed -= HandleInteraction;
    }
}
```

### 5. Use SetEnabled() Instead of Component Disable

For temporary disabling, use SetEnabled():

```csharp
// ✅ Good - Still detected, but shown as disabled
SetEnabled(false);

// ❌ Less flexible - Completely removes from detection
this.enabled = false;
```

---

## Troubleshooting

### Interactions Not Working

1. **Check Module is Installed**: Select PlayerStateMachine → verify InteractionModule is in the modules list
2. **Check Input**: Press E key → verify input is reaching InputBridge (add debug log)
3. **Check Detection Range**: Select player → look for InteractionDetector child GameObject → verify range in Inspector
4. **Check Layers**: Verify interactable's layer is included in module's "Interactable Layers"
5. **Check Collider**: Interactable needs a Collider component (doesn't need to be trigger)

### UI Not Showing

1. **Check UI References**: Select InteractionUIController → verify all UI elements are assigned
2. **Check Canvas**: Verify Canvas is active and visible
3. **Check Module**: Verify PlayerInteractionModule is installed (UI controller will disable itself if not found)

### Wrong Object Being Detected

1. **Check Detection Angle**: Reduce angle if detecting objects behind player
2. **Check Layers**: Use layer masks to filter specific object types
3. **Check Distance**: Object might be closer than expected target

### Module Not Working After Scene Load

1. **Check Installation**: Module Install() is called in Start() - verify PlayerStateMachine starts correctly
2. **Check References**: Serialized references are preserved across scenes

---

## Examples Summary

### Included Examples

Located in `Scripts/Interaction/Examples/`:

1. **SimpleInteractable**: Logs debug messages, optionally destroys itself
2. **EventInteractable**: Fires UnityEvents, supports one-time use
3. **PickupInteractable**: Shows pattern for inventory integration

### Use Cases

- **Doors**: Check locked state, play animations
- **Chests**: Conditional opening, inventory integration
- **NPCs**: Trigger dialogue systems
- **Switches**: Toggle states, activate mechanisms
- **Pickups**: Items, weapons, consumables
- **Vehicles**: Enter/exit vehicles (TakeHelmInteractable pattern from E8)
- **Crafting Stations**: Open crafting UI
- **Shops**: Trigger shopping interface

---

## Architecture Notes

### Why This Design?

1. **Interface-Based**: Easy to extend, no inheritance limitations
2. **Self-Describing UI**: Each interactable defines its presentation (no central switch statement)
3. **Event-Driven**: Loose coupling between systems
4. **Module Pattern**: Works independently, integrates seamlessly
5. **Single Button**: Simplified UX (can be extended if needed)
6. **Validation Hooks**: Context-sensitive interactions (inventory full, locked doors, etc.)

### Compared to Echoes of Eight System

**Improvements**:
- Module-based (can be omitted)
- Single button (simpler)
- Self-describing UI (no type enum needed)
- Better event-driven architecture (C# events + EventManager)
- More flexible API for future modules

**Retained Strengths**:
- Spatial detection (range + angle)
- Closest-first prioritization
- Active component validation
- Clear interface contract

---

## Future Roadmap

Potential enhancements for future versions:

- **Hold-To-Interact**: Timed interactions with progress bar
- **Multi-Step Interactions**: Sequences of interactions
- **Interaction Priorities**: Override detection order
- **Audio Feedback**: Built-in audio cues
- **Animation Integration**: Automatic player animations during interaction
- **Networked Interactions**: Multiplayer support

---

## Support

For issues, questions, or feature requests:
- Check the **Troubleshooting** section above
- Review example interactables in `Scripts/Interaction/Examples/`
- Inspect the module code - it's well-documented!

---

**Version**: 1.0
**Compatible With**: Base Locomotion Module, Player Stats Module
**Unity Version**: 2021.3+
**Input System**: Unity Input System (new)
