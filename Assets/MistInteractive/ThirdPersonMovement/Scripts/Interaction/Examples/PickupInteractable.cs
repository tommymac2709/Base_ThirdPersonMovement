using UnityEngine;

namespace MistInteractive.ThirdPerson.Interaction.Examples
{
    /// <summary>
    /// Example interactable that demonstrates how a pickup would work with a future inventory module.
    /// This shows the pattern for conditional interactions based on module presence.
    /// Currently just logs messages, but can be extended when inventory module is available.
    /// </summary>
    public class PickupInteractable : InteractableBase
    {
        [Header("Pickup Settings")]
        [Tooltip("The name of the item being picked up")]
        [SerializeField] private string itemName = "Item";

        [Tooltip("Visual representation of the item")]
        [SerializeField] private GameObject itemModel;

        [Tooltip("Whether to destroy this object after pickup")]
        [SerializeField] private bool destroyAfterPickup = true;

        public override void Interact(Transform interactor)
        {
            // Get the player state machine to check for inventory module
            var stateMachine = interactor.GetComponent<Player.PlayerStateMachine>();
            if (stateMachine == null)
            {
                Debug.LogWarning($"[PickupInteractable] Interactor {interactor.name} does not have a PlayerStateMachine.");
                return;
            }

            // Example: Check for future inventory module
            // var inventoryModule = stateMachine.GetModule<InventoryModule>();
            // if (inventoryModule != null)
            // {
            //     bool success = inventoryModule.AddItem(itemName);
            //     if (success)
            //     {
            //         OnPickupSuccess();
            //     }
            //     else
            //     {
            //         Debug.Log("[PickupInteractable] Inventory full!");
            //     }
            // }
            // else
            // {
            //     // No inventory module - just log for now
            //     Debug.Log($"[PickupInteractable] No inventory module found. Would pick up: {itemName}");
            //     OnPickupSuccess();
            // }

            // Temporary implementation without inventory module
            Debug.Log($"[PickupInteractable] Picked up {itemName}");
            OnPickupSuccess();
        }

        public override bool CanInteract(Transform interactor)
        {
            if (!base.CanInteract(interactor))
                return false;

            // Example: Check inventory space in future
            // var stateMachine = interactor.GetComponent<Player.PlayerStateMachine>();
            // var inventoryModule = stateMachine?.GetModule<InventoryModule>();
            // if (inventoryModule != null && !inventoryModule.HasSpace())
            // {
            //     return false;
            // }

            return true;
        }

        public override InteractionUIData GetUIData()
        {
            var data = base.GetUIData();

            // Example: Change prompt color to red if inventory full
            // var playerStateMachine = FindFirstObjectByType<Player.PlayerStateMachine>();
            // var inventoryModule = playerStateMachine?.GetModule<InventoryModule>();
            // if (inventoryModule != null && !inventoryModule.HasSpace())
            // {
            //     data.promptColor = Color.red;
            //     data.promptText = $"{itemName} (Inventory Full)";
            // }

            return data;
        }

        private void OnPickupSuccess()
        {
            // Hide or destroy the item
            if (itemModel != null)
            {
                itemModel.SetActive(false);
            }

            if (destroyAfterPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                SetEnabled(false);
            }
        }
    }
}
