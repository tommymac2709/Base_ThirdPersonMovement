using System;
using System.Collections;
using UnityEngine;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Utility;

namespace MistInteractive.ThirdPerson.Stats
{
    /// <summary>
    /// Module that provides mana system for spells and abilities.
    /// Optionally scales max mana with level if PlayerProgressionModule is present.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerManaModule", menuName = "MiST/Player Modules/Mana", order = 4)]
    public class PlayerManaModule : PlayerModule
    {
        #region Configuration Fields

        [Header("Mana Settings")]
        [Tooltip("Maximum mana at level 1")]
        [SerializeField] private float baseMaxMana = 100f;

        [Tooltip("Mana regenerated per second")]
        [SerializeField] private float manaRegenRate = 10f;

        [Tooltip("Seconds to wait after using mana before regeneration starts")]
        [SerializeField] private float manaRegenDelay = 3f;

        #endregion

        #region Runtime State

        private PlayerStateMachine cachedStateMachine;
        private LazyValue<float> currentMana;
        private float lastManaUseTime = -999f;

        #endregion

        #region Properties

        /// <summary>
        /// Current mana value
        /// </summary>
        public float CurrentMana => currentMana?.value ?? MaxMana;

        /// <summary>
        /// Maximum mana (scales with level if PlayerProgressionModule present)
        /// </summary>
        public float MaxMana
        {
            get
            {
                var progression = cachedStateMachine?.GetModule<PlayerProgressionModule>();
                float multiplier = progression?.GetStatMultiplier() ?? 1f;
                return baseMaxMana * multiplier;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when mana changes (passes current mana value)
        /// </summary>
        public event Action<float> OnManaChanged;

        #endregion

        #region Module Lifecycle

        public override void Install(PlayerStateMachine sm)
        {
            cachedStateMachine = sm;

            // Initialize LazyValue
            currentMana = new LazyValue<float>(() => MaxMana);

            // Start regeneration coroutine
            sm.StartCoroutine(ManaRegenCoroutine());

            // Optional: Listen for level-ups to restore mana
            var progression = sm.GetModule<PlayerProgressionModule>();
            if (progression != null && progression.RestoreResourcesOnLevelUp)
            {
                progression.OnLevelUp += () => Restore();
                Debug.Log($"[PlayerManaModule] Installed with Progression - Base Max Mana: {baseMaxMana} (scales with level)");
            }
            else
            {
                Debug.Log($"[PlayerManaModule] Installed - Max Mana: {baseMaxMana}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Attempt to use mana. Returns false if insufficient mana.
        /// </summary>
        /// <param name="amount">Amount of mana to use</param>
        /// <returns>True if mana was used, false if insufficient</returns>
        public bool Use(float amount)
        {
            if (currentMana.value < amount)
            {
                Debug.LogWarning($"[PlayerManaModule] Insufficient mana. Need {amount:F1}, have {currentMana.value:F1}");
                return false;
            }

            currentMana.value -= amount;
            lastManaUseTime = Time.time;

            // Fire events
            OnManaChanged?.Invoke(currentMana.value);
            EventManager.TriggerEvent("Player.ManaChanged", currentMana.value);

            return true;
        }

        /// <summary>
        /// Restore mana by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to restore (0 = restore to max)</param>
        public void Restore(float amount = 0)
        {
            if (amount <= 0)
            {
                currentMana.value = MaxMana;
            }
            else
            {
                currentMana.value = Mathf.Min(MaxMana, currentMana.value + amount);
            }

            OnManaChanged?.Invoke(currentMana.value);
            EventManager.TriggerEvent("Player.ManaChanged", currentMana.value);

            if (amount <= 0)
            {
                Debug.Log($"[PlayerManaModule] Mana fully restored to {MaxMana:F1}");
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator ManaRegenCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);

                if (currentMana.value < MaxMana)
                {
                    // Check if delay has passed since last use
                    if (Time.time - lastManaUseTime >= manaRegenDelay)
                    {
                        float regenAmount = manaRegenRate * 0.1f; // 0.1s intervals
                        currentMana.value = Mathf.Min(MaxMana, currentMana.value + regenAmount);

                        OnManaChanged?.Invoke(currentMana.value);
                        EventManager.TriggerEvent("Player.ManaChanged", currentMana.value);
                    }
                }
            }
        }

        #endregion
    }
}
