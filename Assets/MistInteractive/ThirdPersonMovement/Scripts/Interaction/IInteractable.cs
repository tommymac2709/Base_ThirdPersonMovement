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
    }
}
