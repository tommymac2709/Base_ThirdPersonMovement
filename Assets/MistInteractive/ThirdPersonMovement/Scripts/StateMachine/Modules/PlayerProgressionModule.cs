using System;
using UnityEngine;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Utility;

namespace MistInteractive.ThirdPerson.Stats
{
    /// <summary>
    /// Module that provides experience tracking and leveling system.
    /// Provides stat scaling multipliers to other modules (Health, Stamina, Mana).
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerProgressionModule", menuName = "MiST/Player Modules/Progression", order = 1)]
    public class PlayerProgressionModule : PlayerModule
    {
        #region Configuration Fields

        [Header("Progression Settings")]
        [Tooltip("Starting level")]
        [SerializeField] private int startingLevel = 1;

        [Tooltip("XP required curve - X axis = level, Y axis = XP required for that level")]
        [SerializeField] private AnimationCurve xpRequiredCurve = AnimationCurve.Linear(1, 100, 20, 2000);

        [Tooltip("Percentage increase to max stats per level (e.g., 0.1 = 10% increase)")]
        [SerializeField] private float statIncreasePerLevel = 0.1f;

        [Tooltip("Restore health/stamina/mana to max when leveling up")]
        [SerializeField] private bool restoreResourcesOnLevelUp = true;

        #endregion

        #region Runtime State

        private PlayerStateMachine cachedStateMachine;
        private LazyValue<int> currentLevel;
        private LazyValue<float> currentExperience;

        #endregion

        #region Properties

        /// <summary>
        /// Current player level
        /// </summary>
        public int CurrentLevel => currentLevel?.value ?? startingLevel;

        /// <summary>
        /// Current experience points
        /// </summary>
        public float CurrentXP => currentExperience?.value ?? 0f;

        /// <summary>
        /// XP required to reach the next level
        /// </summary>
        public float ExperienceToNextLevel => xpRequiredCurve.Evaluate(CurrentLevel + 1);

        /// <summary>
        /// Whether resources should be restored on level up (for other modules to check)
        /// </summary>
        public bool RestoreResourcesOnLevelUp => restoreResourcesOnLevelUp;

        #endregion

        #region Events

        /// <summary>
        /// Fired when the player levels up
        /// </summary>
        public event Action OnLevelUp;

        /// <summary>
        /// Fired when experience is gained (passes amount gained)
        /// </summary>
        public event Action<float> OnExperienceGained;

        #endregion

        #region Module Lifecycle

        public override void Install(PlayerStateMachine sm)
        {
            cachedStateMachine = sm;

            // Initialize LazyValues
            currentLevel = new LazyValue<int>(() => startingLevel);
            currentExperience = new LazyValue<float>(() => 0f);

            Debug.Log($"[PlayerProgressionModule] Installed - Starting Level: {CurrentLevel}, Stat Increase Per Level: {statIncreasePerLevel * 100}%");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get the stat multiplier for the current level.
        /// Used by Health/Stamina/Mana modules to scale their max values.
        /// </summary>
        /// <returns>Multiplier (1.0 at level 1, increases with level)</returns>
        public float GetStatMultiplier()
        {
            return 1f + (CurrentLevel - 1) * statIncreasePerLevel;
        }

        /// <summary>
        /// Award experience to the player. Will trigger level up if threshold reached.
        /// </summary>
        /// <param name="amount">Amount of XP to award</param>
        public void GainExperience(float amount)
        {
            currentExperience.value += amount;

            // Fire events
            OnExperienceGained?.Invoke(amount);
            EventManager.TriggerEvent("Player.ExperienceGained", amount);

            Debug.Log($"[PlayerProgressionModule] Gained {amount} XP. Total: {currentExperience.value}/{ExperienceToNextLevel}");

            // Check for level up (can level multiple times if enough XP)
            while (currentExperience.value >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        #endregion

        #region Private Methods

        private void LevelUp()
        {
            // Deduct XP required for level
            currentExperience.value -= ExperienceToNextLevel;

            // Increase level
            currentLevel.value++;

            Debug.Log($"[PlayerProgressionModule] Level Up! Now level {CurrentLevel}");

            // Fire events (other modules listen to restore resources)
            OnLevelUp?.Invoke();
            EventManager.TriggerEvent("Player.LevelUp", CurrentLevel);
        }

        #endregion
    }
}
