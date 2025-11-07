using UnityEngine;

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
    }
}
