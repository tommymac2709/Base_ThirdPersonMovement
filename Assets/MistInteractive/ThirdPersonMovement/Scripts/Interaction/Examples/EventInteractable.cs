using UnityEngine;
using UnityEngine.Events;

namespace MistInteractive.ThirdPerson.Interaction.Examples
{
    /// <summary>
    /// Interactable that fires a UnityEvent when interacted with.
    /// This is designer-friendly and allows hooking up interactions in the Inspector without code.
    /// </summary>
    public class EventInteractable : InteractableBase
    {
        [Header("Event Settings")]
        [Tooltip("Event fired when this object is interacted with")]
        [SerializeField] private UnityEvent onInteract;

        [Header("One-Time Interaction")]
        [Tooltip("If true, can only be interacted with once")]
        [SerializeField] private bool oneTimeUse = false;

        private bool hasBeenUsed = false;

        public override void Interact(Transform interactor)
        {
            if (oneTimeUse && hasBeenUsed)
            {
                Debug.LogWarning($"[EventInteractable] {gameObject.name} has already been used.");
                return;
            }

            onInteract?.Invoke();
            hasBeenUsed = true;

            if (oneTimeUse)
            {
                SetEnabled(false);
                Debug.Log($"[EventInteractable] {gameObject.name} has been disabled after one-time use.");
            }
        }

        public override bool CanInteract(Transform interactor)
        {
            if (oneTimeUse && hasBeenUsed)
                return false;

            return base.CanInteract(interactor);
        }

        /// <summary>
        /// Resets the one-time use flag. Can be called from UnityEvents or other scripts.
        /// </summary>
        public void ResetUse()
        {
            hasBeenUsed = false;
            SetEnabled(true);
        }
    }
}
