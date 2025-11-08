using UnityEngine;

namespace MistInteractive.ThirdPerson.Interaction
{
    /// <summary>
    /// Interface for objects that can be interacted with by the player.
    /// Implement this interface to create custom interactable objects.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when the player interacts with this object.
        /// </summary>
        /// <param name="interactor">The Transform of the player performing the interaction.</param>
        void Interact(Transform interactor);

        /// <summary>
        /// Returns UI data that describes how this interactable should be displayed.
        /// This allows each interactable to customize its appearance in the UI.
        /// </summary>
        /// <returns>UI configuration data for this interactable.</returns>
        InteractionUIData GetUIData();

        /// <summary>
        /// Returns the Transform of this interactable.
        /// Used for distance calculations and positioning.
        /// </summary>
        /// <returns>This interactable's Transform component.</returns>
        Transform GetTransform();

        /// <summary>
        /// Validates whether the interaction can currently be performed.
        /// Use this for context-sensitive interactions (e.g., checking inventory space).
        /// </summary>
        /// <param name="interactor">The Transform of the player attempting the interaction.</param>
        /// <returns>True if the interaction can be performed, false otherwise.</returns>
        bool CanInteract(Transform interactor);

        /// <summary>
        /// Returns a custom interaction range for this interactable, overriding the module default.
        /// Return null to use the module's default range.
        /// </summary>
        /// <returns>Custom range in units, or null for default.</returns>
        float? GetCustomRange();

        /// <summary>
        /// Returns the duration required to hold the interact button.
        /// Return 0 for instant interaction, > 0 for hold-to-interact.
        /// </summary>
        /// <returns>Duration in seconds.</returns>
        float GetInteractionDuration();

        /// <summary>
        /// Returns the priority of this interactable when multiple are at the same distance.
        /// Higher values are preferred. Default should be 0.
        /// </summary>
        /// <returns>Priority value (higher = more important).</returns>
        int GetPriority();

        /// <summary>
        /// Called when this interactable becomes the focused target.
        /// Use for visual feedback (highlighting, glowing, etc.).
        /// </summary>
        void OnFocused();

        /// <summary>
        /// Called when this interactable loses focus.
        /// Use to remove visual feedback.
        /// </summary>
        void OnUnfocused();
    }
}
