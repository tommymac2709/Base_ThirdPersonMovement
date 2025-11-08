using UnityEngine;
using MistInteractive.ThirdPerson.Utils;

namespace MistInteractive.ThirdPerson.Interaction
{
    /// <summary>
    /// Abstract base class for interactable objects.
    /// Provides default implementations of IInteractable for convenience.
    /// Override methods as needed to customize behavior.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [Tooltip("The prompt text displayed to the player")]
        [SerializeField] protected string promptText = "Interact";

        [Tooltip("The button text shown (leave empty to use module default)")]
        [SerializeField] protected string buttonText = "";

        [Tooltip("Color for the prompt text")]
        [SerializeField] protected Color promptColor = Color.white;

        [Tooltip("Optional icon to display")]
        [SerializeField] protected Sprite icon;

        [Tooltip("Whether this interactable is currently enabled")]
        [SerializeField] protected bool enabled = true;

        [Header("Advanced Settings")]
        [Tooltip("Custom interaction range (leave 0 to use module default)")]
        [SerializeField] protected float customRange = 0f;

        [Tooltip("Hold duration in seconds (0 = instant, > 0 = hold to interact)")]
        [SerializeField] protected float interactionDuration = 0f;

        [Tooltip("Priority when multiple interactables are at same distance (higher = preferred)")]
        [SerializeField] protected int priority = 0;

        [Header("Event Manager Integration")]
        [Tooltip("Optional event name to fire when interacted with (uses EventManager)")]
        [SerializeField] protected string eventOnInteract = "";

        [Tooltip("Optional event name to fire when focused")]
        [SerializeField] protected string eventOnFocused = "";

        [Tooltip("Optional event name to fire when unfocused")]
        [SerializeField] protected string eventOnUnfocused = "";

        /// <summary>
        /// Called when the player interacts with this object.
        /// Override this method to implement your interaction logic.
        /// </summary>
        /// <param name="interactor">The Transform of the player performing the interaction.</param>
        public abstract void Interact(Transform interactor);

        /// <summary>
        /// Returns UI data for this interactable.
        /// Override to customize UI presentation dynamically.
        /// </summary>
        public virtual InteractionUIData GetUIData()
        {
            return new InteractionUIData
            {
                promptText = promptText,
                buttonText = buttonText,
                promptColor = promptColor,
                icon = icon,
                enabled = enabled
            };
        }

        /// <summary>
        /// Returns this interactable's Transform.
        /// </summary>
        public virtual Transform GetTransform()
        {
            return transform;
        }

        /// <summary>
        /// Validates whether the interaction can currently be performed.
        /// Override to add custom validation logic (e.g., checking inventory space).
        /// </summary>
        /// <param name="interactor">The Transform of the player attempting the interaction.</param>
        /// <returns>True if the interaction can be performed, false otherwise.</returns>
        public virtual bool CanInteract(Transform interactor)
        {
            return enabled && isActiveAndEnabled;
        }

        /// <summary>
        /// Sets whether this interactable is enabled.
        /// </summary>
        public virtual void SetEnabled(bool value)
        {
            enabled = value;
        }

        /// <summary>
        /// Sets the prompt text dynamically.
        /// </summary>
        public virtual void SetPromptText(string text)
        {
            promptText = text;
        }

        /// <summary>
        /// Returns a custom interaction range, or null to use module default.
        /// </summary>
        public virtual float? GetCustomRange()
        {
            return customRange > 0f ? customRange : (float?)null;
        }

        /// <summary>
        /// Returns the hold duration for this interaction.
        /// 0 = instant, > 0 = hold to interact.
        /// </summary>
        public virtual float GetInteractionDuration()
        {
            return interactionDuration;
        }

        /// <summary>
        /// Returns the priority of this interactable.
        /// Higher values are preferred when multiple are at the same distance.
        /// </summary>
        public virtual int GetPriority()
        {
            return priority;
        }

        /// <summary>
        /// Called when this interactable becomes focused.
        /// Override to add visual effects (highlighting, particles, etc.).
        /// </summary>
        public virtual void OnFocused()
        {
            // Fire EventManager event if configured
            if (!string.IsNullOrWhiteSpace(eventOnFocused))
            {
                EventManager.TriggerEvent(eventOnFocused);
            }
        }

        /// <summary>
        /// Called when this interactable loses focus.
        /// Override to remove visual effects.
        /// </summary>
        public virtual void OnUnfocused()
        {
            // Fire EventManager event if configured
            if (!string.IsNullOrWhiteSpace(eventOnUnfocused))
            {
                EventManager.TriggerEvent(eventOnUnfocused);
            }
        }

        /// <summary>
        /// Helper method to fire interaction event through EventManager.
        /// Call this from your Interact() override if you want to use EventManager.
        /// </summary>
        protected void FireInteractionEvent()
        {
            if (!string.IsNullOrWhiteSpace(eventOnInteract))
            {
                EventManager.TriggerEvent(eventOnInteract);
            }
        }
    }
}
