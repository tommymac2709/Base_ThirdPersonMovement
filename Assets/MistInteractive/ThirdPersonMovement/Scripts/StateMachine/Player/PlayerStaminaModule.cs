using System;
using System.Collections;
using UnityEngine;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Utility;

namespace MistInteractive.ThirdPerson.Stats
{
    /// <summary>
    /// Module that provides stamina system for actions like sprinting, dodging, and blocking.
    /// Optionally scales max stamina with level if PlayerProgressionModule is present.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStaminaModule", menuName = "MiST/Player Modules/Stamina", order = 3)]
    public class PlayerStaminaModule : PlayerModule
    {
        #region Configuration Fields

        [Header("Stamina Settings")]
        [Tooltip("Maximum stamina at level 1")]
        [SerializeField] private float baseMaxStamina = 100f;

        [Tooltip("Stamina regenerated per second")]
        [SerializeField] private float staminaRegenRate = 20f;

        [Tooltip("Seconds to wait after using stamina before regeneration starts")]
        [SerializeField] private float staminaRegenDelay = 2f;

        #endregion

        #region Runtime State

        private PlayerStateMachine cachedStateMachine;
        private LazyValue<float> currentStamina;
        private float lastStaminaUseTime = -999f;

        #endregion

        #region Properties

        /// <summary>
        /// Current stamina value
        /// </summary>
        public float CurrentStamina => currentStamina?.value ?? MaxStamina;

        /// <summary>
        /// Maximum stamina (scales with level if PlayerProgressionModule present)
        /// </summary>
        public float MaxStamina
        {
            get
            {
                var progression = cachedStateMachine?.GetModule<PlayerProgressionModule>();
                float multiplier = progression?.GetStatMultiplier() ?? 1f;
                return baseMaxStamina * multiplier;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when stamina changes (passes current stamina value)
        /// </summary>
        public event Action<float> OnStaminaChanged;

        /// <summary>
        /// Fired when stamina reaches zero
        /// </summary>
        public event Action OnStaminaDepleted;

        #endregion

        #region Module Lifecycle

        public override void Install(PlayerStateMachine sm)
        {
            cachedStateMachine = sm;

            // Initialize LazyValue
            currentStamina = new LazyValue<float>(() => MaxStamina);

            // Start regeneration coroutine
            sm.StartCoroutine(StaminaRegenCoroutine());

            // Optional: Listen for level-ups to restore stamina
            var progression = sm.GetModule<PlayerProgressionModule>();
            if (progression != null && progression.RestoreResourcesOnLevelUp)
            {
                progression.OnLevelUp += () => Restore();
                Debug.Log($"[PlayerStaminaModule] Installed with Progression - Base Max Stamina: {baseMaxStamina} (scales with level)");
            }
            else
            {
                Debug.Log($"[PlayerStaminaModule] Installed - Max Stamina: {baseMaxStamina}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Attempt to use stamina. Returns false if insufficient stamina.
        /// </summary>
        /// <param name="amount">Amount of stamina to use</param>
        /// <returns>True if stamina was used, false if insufficient</returns>
        public bool Use(float amount)
        {
            if (currentStamina.value < amount)
            {
                Debug.LogWarning($"[PlayerStaminaModule] Insufficient stamina. Need {amount:F1}, have {currentStamina.value:F1}");
                return false;
            }

            currentStamina.value -= amount;
            lastStaminaUseTime = Time.time;

            // Fire events
            OnStaminaChanged?.Invoke(currentStamina.value);

            if (currentStamina.value <= 0)
            {
                OnStaminaDepleted?.Invoke();
                EventManager.TriggerEvent("Player.StaminaDepleted");
                Debug.Log("[PlayerStaminaModule] Stamina depleted!");
            }

            return true;
        }

        /// <summary>
        /// Restore stamina by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to restore (0 = restore to max)</param>
        public void Restore(float amount = 0)
        {
            if (amount <= 0)
            {
                currentStamina.value = MaxStamina;
            }
            else
            {
                currentStamina.value = Mathf.Min(MaxStamina, currentStamina.value + amount);
            }

            OnStaminaChanged?.Invoke(currentStamina.value);

            if (amount <= 0)
            {
                Debug.Log($"[PlayerStaminaModule] Stamina fully restored to {MaxStamina:F1}");
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator StaminaRegenCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);

                if (currentStamina.value < MaxStamina)
                {
                    // Check if delay has passed since last use
                    if (Time.time - lastStaminaUseTime >= staminaRegenDelay)
                    {
                        float regenAmount = staminaRegenRate * 0.1f; // 0.1s intervals
                        currentStamina.value = Mathf.Min(MaxStamina, currentStamina.value + regenAmount);

                        OnStaminaChanged?.Invoke(currentStamina.value);
                    }
                }
            }
        }

        #endregion
    }
}
