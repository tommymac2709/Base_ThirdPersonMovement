using UnityEngine;

namespace MistInteractive.ThirdPerson.Interaction
{
    /// <summary>
    /// Data structure that defines how an interactable should be displayed in the UI.
    /// Each interactable returns this data to customize its visual presentation.
    /// </summary>
    [System.Serializable]
    public struct InteractionUIData
    {
        /// <summary>
        /// The main prompt text displayed to the player (e.g., "Open Chest", "Talk to Merchant").
        /// </summary>
        [Tooltip("The main prompt text displayed to the player")]
        public string promptText;

        /// <summary>
        /// The button/key text shown (e.g., "E", "Interact").
        /// </summary>
        [Tooltip("The button/key text shown")]
        public string buttonText;

        /// <summary>
        /// Color for the prompt text. Can be used for visual feedback (e.g., red when unavailable).
        /// </summary>
        [Tooltip("Color for the prompt text")]
        public Color promptColor;

        /// <summary>
        /// Optional icon sprite to display alongside the prompt.
        /// </summary>
        [Tooltip("Optional icon sprite to display")]
        public Sprite icon;

        /// <summary>
        /// Whether the interaction is currently enabled.
        /// Set to false to visually disable the interaction while still detecting it.
        /// </summary>
        [Tooltip("Whether the interaction is currently enabled")]
        public bool enabled;

        /// <summary>
        /// Creates a default InteractionUIData with common settings.
        /// </summary>
        /// <param name="prompt">The prompt text to display.</param>
        /// <param name="button">The button text (defaults to "E").</param>
        /// <returns>A new InteractionUIData instance.</returns>
        public static InteractionUIData Create(string prompt, string button = "E")
        {
            return new InteractionUIData
            {
                promptText = prompt,
                buttonText = button,
                promptColor = Color.white,
                icon = null,
                enabled = true
            };
        }
    }
}
