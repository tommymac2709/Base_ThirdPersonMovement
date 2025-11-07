using System;
using UnityEngine;
using MistInteractive.ThirdPerson.Interaction;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// Module that enables interaction with objects in the game world.
    /// Handles detection, validation, and execution of interactions with IInteractable objects.
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

        #endregion

        #region Private Fields

        private PlayerStateMachine cachedStateMachine;
        private InteractionDetector detector;
        private IInteractable currentInteractable;
        private bool interactionEnabled = true;
        private Control.InputBridge inputBridge;

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
            }
            else
            {
                Debug.LogWarning("[PlayerInteractionModule] InputBridge not found on PlayerStateMachine. Interaction input will not work.");
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
            }

            if (detector != null)
            {
                detector.OnDetectedInteractableChanged -= HandleInteractableChanged;
            }
        }

        /// <summary>
        /// Called when the interact input is pressed.
        /// </summary>
        private void OnInteractInput()
        {
            TryInteract();
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
        /// </summary>
        /// <returns>True if an interaction was performed, false otherwise.</returns>
        public bool TryInteract()
        {
            if (!interactionEnabled || currentInteractable == null)
                return false;

            if (!currentInteractable.CanInteract(cachedStateMachine.transform))
                return false;

            currentInteractable.Interact(cachedStateMachine.transform);
            OnInteractionPerformed?.Invoke(currentInteractable, cachedStateMachine.transform);
            return true;
        }

        /// <summary>
        /// Enables or disables interaction system.
        /// When disabled, interactions cannot be triggered but detection still occurs.
        /// </summary>
        /// <param name="enabled">Whether interactions should be enabled.</param>
        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;
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
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Called by InteractionDetector when the focused interactable changes.
        /// </summary>
        internal void HandleInteractableChanged(IInteractable newInteractable)
        {
            if (currentInteractable != newInteractable)
            {
                if (newInteractable != null)
                {
                    currentInteractable = newInteractable;
                    OnInteractableDetected?.Invoke(currentInteractable);
                }
                else
                {
                    currentInteractable = null;
                    OnInteractableLost?.Invoke();
                }
            }
        }

        #endregion
    }
}
