using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input and bridges Unity's Input System events to the game's systems.
/// Implements IPlayerActions for automatic input callback wiring.
/// </summary>
public class InputReader : MonoBehaviour, Controls.IPlayerActions, Controls.IUIActions
{
    public bool IsSprinting { get; private set; }
    // Current value of the player movement input (Vector2: x = left/right, y = forward/backward).
    public Vector2 MovementValue { get; private set; }

    // Player events
    public event Action CancelWindowEvent;
    public event Action JumpEvent;
    public event Action PauseEvent; // New pause event

    // UI/Menu events  
    public event Action SubmitEvent;
    public event Action<Vector2> NavigateEvent;

    public Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);
        controls.UI.SetCallbacks(this);
        controls.Player.Enable();
        controls.UI.Enable();

        
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions and ensure controls are disabled.
 
        controls.Player.Disable();
        controls.UI.Disable();
    }

    /// <summary>
    /// Disables both Player and UI input controls.
    /// </summary>
    private void DisableAllControls()
    {
        controls.Player.Disable();
        controls.UI.Disable();
    }

    /// <summary>
    /// Disables player controls only (used when a UI window is open, for example).
    /// </summary>
    public void DisableControls()
    {
        controls.Player.Disable();
    }

    /// <summary>
    /// Enables player controls only (used when all UI windows are closed).
    /// </summary>
    public void EnableControls()
    {
        controls.Player.Enable();
    }

    // --------- Player Input System Callback Implementations ----------
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        JumpEvent?.Invoke();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Intentionally left empty – implement look functionality as needed.
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsSprinting = true;
        }
        else if (context.canceled)
        {
            IsSprinting = false;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        PauseEvent?.Invoke();
    }

    // --------- UI Input System Callback Implementations ----------
    public void OnCancelWindow(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        CancelWindowEvent?.Invoke();
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        SubmitEvent?.Invoke();
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        NavigateEvent?.Invoke(context.ReadValue<Vector2>());
    }
}