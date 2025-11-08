using System;
using UnityEngine;
using MistInteractive.ThirdPerson.Interaction;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// Module that enables interaction with objects in the game world.
    /// Handles detection, validation, and execution of interactions with IInteractable objects.
    /// Supports instant and hold-to-interact, custom ranges, priorities, and cycling.
    /// Works independently and provides a flexible API for future modules (inventory, combat, etc.).
    /// </summary>
    [CreateAssetMenu(menuName = "MiST/Player Modules/Interaction", fileName = "InteractionModule")]
    public class PlayerInteractionModule : PlayerModule
    {
        #region Settings

        [Header("Detection Settings")]
        [Tooltip("Maximum distance at which objects can be detected for interaction")]
        [SerializeField] private float interactionRange = 3f;

        [Tooltip("Forward-facing detection angle in degrees (90 = front quarter sphere)")]
        [SerializeField][Range(0f, 180f)] private float detectionAngle = 90f;

        [Tooltip("Layers that can contain interactable objects")]
        [SerializeField] private LayerMask interactableLayers = ~0; // All layers by default

        [Header("Input Settings")]
        [Tooltip("The key name displayed in UI (e.g., 'E', 'F', etc.)")]
        [SerializeField] private string interactKeyName = "E";

        #endregion

        #region Events

        /// <summary>
        /// Fired when a new interactable enters detection range and is focused.
        /// </summary>
        public event Action<IInteractable> OnInteractableDetected;

        /// <summary>
        /// Fired when the currently focused interactable leaves detection range or is lost.
        /// </summary>
        public event Action OnInteractableLost;

        /// <summary>
        /// Fired when an interaction is performed.
        /// Parameters: the interactable that was interacted with, and the player's transform.
        /// </summary>
        public event Action<IInteractable, Transform> OnInteractionPerformed;

        /// <summary>
        /// Fired every frame during a hold interaction to report progress (0-1).
        /// </summary>
        public event Action<float> OnHoldProgress;

        #endregion

        #region Private Fields

        private PlayerStateMachine cachedStateMachine;
        private InteractionDetector detector;
        private IInteractable currentInteractable;
        private bool interactionEnabled = true;
        private Control.InputBridge inputBridge;

        // Hold-to-interact state
        private bool isHolding = false;
        private float holdStartTime = 0f;
        private float requiredHoldDuration = 0f;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the maximum interaction range.
        /// </summary>
        public float InteractionRange
        {
            get => interactionRange;
            set
            {
                interactionRange = Mathf.Max(0f, value);
                if (detector != null)
                    detector.SetRange(interactionRange);
            }
        }

        /// <summary>
        /// Gets or sets the detection angle.
        /// </summary>
        public float DetectionAngle
        {
            get => detectionAngle;
            set
            {
                detectionAngle = Mathf.Clamp(value, 0f, 180f);
                if (detector != null)
                    detector.SetAngle(detectionAngle);
            }
        }

        /// <summary>
        /// Gets the default key name displayed in UI.
        /// </summary>
        public string InteractKeyName => interactKeyName;

        /// <summary>
        /// Gets whether interactions are currently enabled.
        /// </summary>
        public bool IsInteractionEnabled => interactionEnabled;

        /// <summary>
        /// Gets whether the player is currently holding the interact button for a timed interaction.
        /// </summary>
        public bool IsHoldingInteract => isHolding;

        /// <summary>
        /// Gets the current hold progress (0-1).
        /// </summary>
        public float HoldProgress
        {
            get
            {
                if (!isHolding || requiredHoldDuration <= 0f)
                    return 0f;

                return Mathf.Clamp01((Time.time - holdStartTime) / requiredHoldDuration);
            }
        }

        #endregion

        #region Module Lifecycle

        /// <summary>
        /// Installs the interaction module into the player state machine.
        /// Creates and configures the interaction detector component.
        /// </summary>
        /// <param name="sm">The PlayerStateMachine to install into.</param>
        public override void Install(PlayerStateMachine sm)
        {
            cachedStateMachine = sm;

            // Create detector component
            GameObject detectorObj = new GameObject("InteractionDetector");
            detectorObj.transform.SetParent(sm.transform);
            detectorObj.transform.localPosition = Vector3.zero;

            detector = detectorObj.AddComponent<InteractionDetector>();
            detector.Initialize(this, sm.transform, interactionRange, detectionAngle, interactableLayers);

            // Subscribe to detector events
            detector.OnDetectedInteractableChanged += HandleInteractableChanged;

            // Subscribe to input events
            inputBridge = sm.InputBridge;
            if (inputBridge != null)
            {
                inputBridge.InteractEvent += OnInteractInput;
                inputBridge.CycleInteractableEvent += OnCycleInput;
            }
            else
            {
                Debug.LogWarning("[PlayerInteractionModule] InputBridge not found on PlayerStateMachine. Interaction input will not work.");
            }

            // Start update coroutine for hold tracking
            if (detectorObj.TryGetComponent<MonoBehaviour>(out var mb))
            {
                sm.StartCoroutine(HoldUpdateCoroutine());
            }
        }

        /// <summary>
        /// Called when the module is being destroyed or disabled.
        /// Unsubscribes from events to prevent memory leaks.
        /// </summary>
        private void OnDisable()
        {
            if (inputBridge != null)
            {
                inputBridge.InteractEvent -= OnInteractInput;
                inputBridge.CycleInteractableEvent -= OnCycleInput;
            }

            if (detector != null)
            {
                detector.OnDetectedInteractableChanged -= HandleInteractableChanged;
            }

            CancelHold();
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// Called when the interact input is pressed.
        /// Starts instant or hold interaction based on interactable's duration.
        /// </summary>
        private void OnInteractInput()
        {
            if (!interactionEnabled || currentInteractable == null)
                return;

            if (!currentInteractable.CanInteract(cachedStateMachine.transform))
                return;

            // Get the required hold duration
            requiredHoldDuration = currentInteractable.GetInteractionDuration();

            if (requiredHoldDuration <= 0f)
            {
                // Instant interaction
                PerformInteraction();
            }
            else
            {
                // Start hold interaction
                StartHold();
            }
        }

        /// <summary>
        /// Called when the cycle interactable input is pressed.
        /// </summary>
        private void OnCycleInput()
        {
            CycleNext();
        }

        #endregion

        #region Hold-to-Interact Logic

        /// <summary>
        /// Starts a hold interaction.
        /// </summary>
        private void StartHold()
        {
            isHolding = true;
            holdStartTime = Time.time;
        }

        /// <summary>
        /// Cancels the current hold interaction.
        /// </summary>
        private void CancelHold()
        {
            if (isHolding)
            {
                isHolding = false;
                OnHoldProgress?.Invoke(0f);
            }
        }

        /// <summary>
        /// Coroutine that tracks hold progress and completes interaction when time is reached.
        /// </summary>
        private System.Collections.IEnumerator HoldUpdateCoroutine()
        {
            while (true)
            {
                if (isHolding)
                {
                    // Check if interact button is still held
                    bool stillHolding = inputBridge != null && inputBridge.controls.Player.Interact.IsPressed();

                    if (!stillHolding)
                    {
                        // Button released - cancel hold
                        CancelHold();
                    }
                    else if (currentInteractable == null)
                    {
                        // Lost target - cancel hold
                        CancelHold();
                    }
                    else
                    {
                        // Update progress
                        float progress = HoldProgress;
                        OnHoldProgress?.Invoke(progress);

                        // Check if hold is complete
                        if (progress >= 1f)
                        {
                            PerformInteraction();
                            isHolding = false;
                            OnHoldProgress?.Invoke(0f);
                        }
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Performs the actual interaction.
        /// </summary>
        private void PerformInteraction()
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact(cachedStateMachine.transform);
                OnInteractionPerformed?.Invoke(currentInteractable, cachedStateMachine.transform);
            }
        }

        #endregion

        #region Cycling

        /// <summary>
        /// Cycles to the next valid interactable.
        /// </summary>
        public void CycleNext()
        {
            if (detector != null)
            {
                detector.CycleNext();
            }
        }

        /// <summary>
        /// Cycles to the previous valid interactable.
        /// </summary>
        public void CyclePrevious()
        {
            if (detector != null)
            {
                detector.CyclePrevious();
            }
        }

        /// <summary>
        /// Gets the number of currently valid interactables in range.
        /// </summary>
        public int GetInteractableCount()
        {
            return detector != null ? detector.GetInteractableCount() : 0;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the currently detected interactable object.
        /// </summary>
        /// <returns>The current interactable, or null if none is detected.</returns>
        public IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }

        /// <summary>
        /// Attempts to interact with the currently detected interactable.
        /// For manual triggering (not from input).
        /// </summary>
        /// <returns>True if an interaction was started, false otherwise.</returns>
        public bool TryInteract()
        {
            if (!interactionEnabled || currentInteractable == null)
                return false;

            if (!currentInteractable.CanInteract(cachedStateMachine.transform))
                return false;

            requiredHoldDuration = currentInteractable.GetInteractionDuration();

            if (requiredHoldDuration <= 0f)
            {
                PerformInteraction();
                return true;
            }
            else
            {
                StartHold();
                return true;
            }
        }

        /// <summary>
        /// Enables or disables interaction system.
        /// When disabled, interactions cannot be triggered but detection still occurs.
        /// </summary>
        /// <param name="enabled">Whether interactions should be enabled.</param>
        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;

            if (!enabled)
            {
                CancelHold();
            }
        }

        /// <summary>
        /// Manually sets the detection active state.
        /// Use this to completely disable/enable detection (not just interaction).
        /// </summary>
        /// <param name="active">Whether detection should be active.</param>
        public void SetDetectionActive(bool active)
        {
            if (detector != null)
                detector.SetActive(active);

            if (!active)
            {
                CancelHold();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Called by InteractionDetector when the focused interactable changes.
        /// </summary>
        internal void HandleInteractableChanged(IInteractable newInteractable)
        {
            // Call OnUnfocused on old interactable
            if (currentInteractable != null && currentInteractable != newInteractable)
            {
                currentInteractable.OnUnfocused();
                CancelHold(); // Cancel any ongoing hold
            }

            currentInteractable = newInteractable;

            // Call OnFocused on new interactable
            if (currentInteractable != null)
            {
                currentInteractable.OnFocused();
                OnInteractableDetected?.Invoke(currentInteractable);
            }
            else
            {
                OnInteractableLost?.Invoke();
            }
        }

        #endregion
    }
}
