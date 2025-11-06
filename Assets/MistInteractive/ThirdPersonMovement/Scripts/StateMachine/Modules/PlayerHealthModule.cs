using System;
using System.Collections;
using UnityEngine;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Utility;

namespace MistInteractive.ThirdPerson.Stats
{
    /// <summary>
    /// Module that provides health system with damage, healing, death, and auto-regeneration.
    /// Optionally scales max health with level if PlayerProgressionModule is present.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerHealthModule", menuName = "MiST/Player Modules/Health", order = 2)]
    public class PlayerHealthModule : PlayerModule
    {
        #region Configuration Fields

        [Header("Health Settings")]
        [Tooltip("Maximum health at level 1")]
        [SerializeField] private float baseMaxHealth = 100f;

        [Tooltip("Health regenerated per second")]
        [SerializeField] private float healthRegenRate = 5f;

        [Tooltip("Seconds to wait after taking damage before regeneration starts")]
        [SerializeField] private float healthRegenDelay = 5f;

        #endregion

        #region Runtime State

        private PlayerStateMachine cachedStateMachine;
        private LazyValue<float> currentHealth;
        private float lastDamageTime = -999f;

        #endregion

        #region Properties

        /// <summary>
        /// Current health value
        /// </summary>
        public float CurrentHealth => currentHealth?.value ?? MaxHealth;

        /// <summary>
        /// Maximum health (scales with level if PlayerProgressionModule present)
        /// </summary>
        public float MaxHealth
        {
            get
            {
                var progression = cachedStateMachine?.GetModule<PlayerProgressionModule>();
                float multiplier = progression?.GetStatMultiplier() ?? 1f;
                return baseMaxHealth * multiplier;
            }
        }

        /// <summary>
        /// Is the player dead?
        /// </summary>
        public bool IsDead { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Fired when health changes (passes current health value)
        /// </summary>
        public event Action<float> OnHealthChanged;

        /// <summary>
        /// Fired when player dies
        /// </summary>
        public event Action OnDeath;

        #endregion

        #region Module Lifecycle

        public override void Install(PlayerStateMachine sm)
        {
            cachedStateMachine = sm;

            // Initialize LazyValue
            currentHealth = new LazyValue<float>(() => MaxHealth);
            IsDead = false;

            // Start regeneration coroutine
            sm.StartCoroutine(HealthRegenCoroutine());

            // Optional: Listen for level-ups to restore health
            var progression = sm.GetModule<PlayerProgressionModule>();
            if (progression != null && progression.RestoreResourcesOnLevelUp)
            {
                progression.OnLevelUp += RestoreHealth;
                Debug.Log($"[PlayerHealthModule] Installed with Progression - Base Max HP: {baseMaxHealth} (scales with level)");
            }
            else
            {
                Debug.Log($"[PlayerHealthModule] Installed - Max HP: {baseMaxHealth}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Deal damage to the player. Interrupts health regeneration.
        /// </summary>
        /// <param name="damage">Amount of damage to deal</param>
        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            float previousHealth = currentHealth.value;
            currentHealth.value = Mathf.Max(0, currentHealth.value - damage);
            lastDamageTime = Time.time;

            // Fire events
            OnHealthChanged?.Invoke(currentHealth.value);
            EventManager.TriggerEvent("Player.HealthChanged", currentHealth.value);

            Debug.Log($"[PlayerHealthModule] Took {damage} damage. HP: {currentHealth.value:F1}/{MaxHealth:F1}");

            // Check for death
            if (currentHealth.value <= 0 && !IsDead)
            {
                HandleDeath();
            }
        }

        /// <summary>
        /// Heal the player by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(float amount)
        {
            if (IsDead) return;

            currentHealth.value = Mathf.Min(MaxHealth, currentHealth.value + amount);

            // Fire events
            OnHealthChanged?.Invoke(currentHealth.value);
            EventManager.TriggerEvent("Player.HealthChanged", currentHealth.value);

            Debug.Log($"[PlayerHealthModule] Healed {amount}. HP: {currentHealth.value:F1}/{MaxHealth:F1}");
        }

        /// <summary>
        /// Fully restore health to maximum.
        /// </summary>
        public void RestoreHealth()
        {
            if (IsDead) return;

            currentHealth.value = MaxHealth;

            // Fire events
            OnHealthChanged?.Invoke(currentHealth.value);
            EventManager.TriggerEvent("Player.HealthChanged", currentHealth.value);

            Debug.Log($"[PlayerHealthModule] Health fully restored to {MaxHealth:F1}");
        }

        /// <summary>
        /// Revive the player with specified health amount.
        /// </summary>
        /// <param name="healthAmount">Amount of health to revive with (0 = full health)</param>
        public void Revive(float healthAmount = 0)
        {
            if (!IsDead) return;

            IsDead = false;
            currentHealth.value = healthAmount > 0 ? healthAmount : MaxHealth;

            OnHealthChanged?.Invoke(currentHealth.value);
            EventManager.TriggerEvent("Player.HealthChanged", currentHealth.value);

            Debug.Log($"[PlayerHealthModule] Player revived with {currentHealth.value:F1} HP");
        }

        #endregion

        #region Private Methods

        private void HandleDeath()
        {
            IsDead = true;

            Debug.Log("[PlayerHealthModule] Player died!");

            // Fire events
            OnDeath?.Invoke();
            EventManager.TriggerEvent("Player.Death");
        }

        private IEnumerator HealthRegenCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);

                if (!IsDead && currentHealth.value < MaxHealth)
                {
                    // Check if delay has passed since last damage
                    if (Time.time - lastDamageTime >= healthRegenDelay)
                    {
                        float regenAmount = healthRegenRate * 0.1f; // 0.1s intervals
                        currentHealth.value = Mathf.Min(MaxHealth, currentHealth.value + regenAmount);

                        OnHealthChanged?.Invoke(currentHealth.value);
                        EventManager.TriggerEvent("Player.HealthChanged", currentHealth.value);
                    }
                }
            }
        }

        #endregion
    }
}
