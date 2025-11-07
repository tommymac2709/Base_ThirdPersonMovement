using UnityEngine;

namespace MistInteractive.ThirdPerson.Interaction.Examples
{
    /// <summary>
    /// Simple example interactable that logs a debug message when interacted with.
    /// Use this as a starting point for creating your own interactables.
    /// </summary>
    public class SimpleInteractable : InteractableBase
    {
        [Header("Simple Interactable Settings")]
        [Tooltip("The message to log when interacted with")]
        [SerializeField] private string debugMessage = "Object was interacted with!";

        [Tooltip("Whether to destroy this object after interaction")]
        [SerializeField] private bool destroyAfterInteraction = false;

        public override void Interact(Transform interactor)
        {
            Debug.Log($"[SimpleInteractable] {debugMessage} (Interactor: {interactor.name})");

            if (destroyAfterInteraction)
            {
                Debug.Log($"[SimpleInteractable] Destroying {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }
}
