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

    public event Action CancelWindowEvent;

    public event Action JumpEvent;

    public Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);
        controls.UI.SetCallbacks(this);

        controls.Player.Enable();
        controls.UI.Enable();

        // Subscribe to UI window events to disable/enable controls as needed.
        WindowController.OnAnyWindowOpened += DisableControls;
        WindowController.OnAllWindowsClosed += EnableControls;
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions and ensure controls are disabled.
        WindowController.OnAnyWindowOpened -= DisableControls;
        WindowController.OnAllWindowsClosed -= EnableControls;

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

    // --------- Input System Callback Implementations ----------

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

    public void OnCancelWindow(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        CancelWindowEvent?.Invoke();
    }
}
