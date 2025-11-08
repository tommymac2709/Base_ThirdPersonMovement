using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Interaction;

namespace MistInteractive.ThirdPersonMovement.UI
{
    /// <summary>
    /// UI controller for displaying interaction prompts to the player.
    /// Automatically subscribes to the PlayerInteractionModule and updates based on detected interactables.
    /// Supports hold-to-interact progress and cycling through multiple interactables.
    /// Gracefully handles missing module - disables itself if module not found.
    /// </summary>
    public class InteractionUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("The main container for all interaction UI elements")]
        [SerializeField] private GameObject container;

        [Tooltip("Text displaying the interaction prompt (e.g., 'Open Chest')")]
        [SerializeField] private TextMeshProUGUI promptText;

        [Tooltip("Text displaying the button/key to press (e.g., 'E')")]
        [SerializeField] private TextMeshProUGUI buttonText;

        [Tooltip("Optional icon image")]
        [SerializeField] private Image iconImage;

        [Header("Hold-to-Interact UI (Optional)")]
        [Tooltip("Optional slider showing hold progress (0-1)")]
        [SerializeField] private Slider holdProgressSlider;

        [Tooltip("Optional text showing hold percentage (e.g., '75%')")]
        [SerializeField] private TextMeshProUGUI holdProgressText;

        [Tooltip("Optional container for hold progress UI elements")]
        [SerializeField] private GameObject holdProgressContainer;

        [Header("Cycling UI (Optional)")]
        [Tooltip("Optional text showing cycle count (e.g., '2/5')")]
        [SerializeField] private TextMeshProUGUI cycleCountText;

        [Tooltip("Optional key hint for cycling (e.g., 'Q to cycle')")]
        [SerializeField] private TextMeshProUGUI cycleHintText;

        [Header("Visual Feedback")]
        [Tooltip("Color used when interaction is disabled")]
        [SerializeField] private Color disabledColor = new Color(1f, 0.5f, 0.5f, 0.7f);

        [Header("References")]
        [Tooltip("Reference to the player state machine (auto-detected if not assigned)")]
        [SerializeField] private PlayerStateMachine playerStateMachine;

        private PlayerInteractionModule interactionModule;
        private IInteractable currentInteractable;

        private void Start()
        {
            // Find PlayerStateMachine if not assigned
            if (playerStateMachine == null)
            {
                playerStateMachine = FindFirstObjectByType<PlayerStateMachine>();
                if (playerStateMachine == null)
                {
                    Debug.LogError("[InteractionUIController] Could not find PlayerStateMachine in scene!");
                    enabled = false;
                    return;
                }
            }

            // Get interaction module
            interactionModule = playerStateMachine.GetModule<PlayerInteractionModule>();
            if (interactionModule == null)
            {
                Debug.LogWarning("[InteractionUIController] PlayerInteractionModule not found. Interaction UI disabled.");
                if (container != null)
                    container.SetActive(false);
                enabled = false;
                return;
            }

            // Subscribe to module events
            interactionModule.OnInteractableDetected += OnInteractableDetected;
            interactionModule.OnInteractableLost += OnInteractableLost;
            interactionModule.OnHoldProgress += OnHoldProgressUpdate;

            // Initialize UI as hidden
            if (container != null)
                container.SetActive(false);

            if (holdProgressContainer != null)
                holdProgressContainer.SetActive(false);

            Debug.Log("[InteractionUIController] Initialized successfully.");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (interactionModule != null)
            {
                interactionModule.OnInteractableDetected -= OnInteractableDetected;
                interactionModule.OnInteractableLost -= OnInteractableLost;
                interactionModule.OnHoldProgress -= OnHoldProgressUpdate;
            }
        }

        private void Update()
        {
            // Update UI every frame in case CanInteract() state changes
            if (currentInteractable != null)
            {
                UpdateUI(currentInteractable);
                UpdateCycleUI();
            }
        }

        private void OnInteractableDetected(IInteractable interactable)
        {
            currentInteractable = interactable;
            UpdateUI(interactable);
            UpdateCycleUI();

            if (container != null)
                container.SetActive(true);
        }

        private void OnInteractableLost()
        {
            currentInteractable = null;

            if (container != null)
                container.SetActive(false);

            if (holdProgressContainer != null)
                holdProgressContainer.SetActive(false);
        }

        private void OnHoldProgressUpdate(float progress)
        {
            // Show/hide hold progress UI
            if (holdProgressContainer != null)
            {
                holdProgressContainer.SetActive(progress > 0f);
            }

            // Update slider
            if (holdProgressSlider != null)
            {
                holdProgressSlider.value = progress;
            }

            // Update text
            if (holdProgressText != null)
            {
                if (progress > 0f)
                {
                    holdProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                }
                else
                {
                    holdProgressText.text = "";
                }
            }
        }

        private void UpdateUI(IInteractable interactable)
        {
            if (interactable == null)
                return;

            // Get UI data from interactable
            InteractionUIData uiData = interactable.GetUIData();

            // Check if interaction can be performed
            bool canInteract = interactable.CanInteract(playerStateMachine.transform) &&
                               interactionModule.IsInteractionEnabled;

            // Update prompt text
            if (promptText != null)
            {
                promptText.text = uiData.promptText;
                promptText.color = canInteract ? uiData.promptColor : disabledColor;
            }

            // Update button text
            if (buttonText != null)
            {
                // Use interactable's button text if provided, otherwise use module default
                string displayButtonText = string.IsNullOrEmpty(uiData.buttonText)
                    ? interactionModule.InteractKeyName
                    : uiData.buttonText;

                // Add "Hold" prefix if this is a hold interaction
                float duration = interactable.GetInteractionDuration();
                if (duration > 0f)
                {
                    displayButtonText = $"Hold {displayButtonText}";
                }

                buttonText.text = displayButtonText;
                buttonText.color = canInteract ? Color.white : disabledColor;
            }

            // Update icon
            if (iconImage != null)
            {
                if (uiData.icon != null)
                {
                    iconImage.sprite = uiData.icon;
                    iconImage.enabled = true;
                    iconImage.color = canInteract ? Color.white : disabledColor;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            // Handle enabled state
            if (!uiData.enabled || !canInteract)
            {
                // Visual feedback that interaction is disabled but detected
                if (promptText != null)
                    promptText.color = disabledColor;
            }
        }

        private void UpdateCycleUI()
        {
            int totalCount = interactionModule.GetInteractableCount();

            // Update cycle count text (e.g., "1/3")
            if (cycleCountText != null)
            {
                if (totalCount > 1)
                {
                    cycleCountText.text = $"1/{totalCount}"; // Always show 1 as we're focused on the primary
                    cycleCountText.gameObject.SetActive(true);
                }
                else
                {
                    cycleCountText.gameObject.SetActive(false);
                }
            }

            // Update cycle hint text
            if (cycleHintText != null)
            {
                if (totalCount > 1)
                {
                    cycleHintText.text = "Q to cycle";
                    cycleHintText.gameObject.SetActive(true);
                }
                else
                {
                    cycleHintText.gameObject.SetActive(false);
                }
            }
        }

        #region Context Menu Tests

        [ContextMenu("Toggle Interaction")]
        void TestToggleInteraction()
        {
            if (interactionModule != null)
            {
                bool newState = !interactionModule.IsInteractionEnabled;
                interactionModule.SetInteractionEnabled(newState);
                Debug.Log($"[InteractionUIController] Interaction enabled: {newState}");
            }
        }

        [ContextMenu("Toggle Detection")]
        void TestToggleDetection()
        {
            if (interactionModule != null)
            {
                // This will disable detection entirely
                interactionModule.SetDetectionActive(false);
                Debug.Log("[InteractionUIController] Detection disabled");
            }
        }

        [ContextMenu("Cycle Next")]
        void TestCycleNext()
        {
            if (interactionModule != null)
            {
                interactionModule.CycleNext();
                Debug.Log($"[InteractionUIController] Cycled to next interactable (total: {interactionModule.GetInteractableCount()})");
            }
        }

        #endregion
    }
}
