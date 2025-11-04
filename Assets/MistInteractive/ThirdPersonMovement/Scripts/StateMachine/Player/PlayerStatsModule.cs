using System;
using System.Collections;
using UnityEngine;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Utility;

namespace MistInteractive.ThirdPerson.Stats
{
    /// <summary>
    /// Module that provides health, stamina, mana resources and experience/leveling progression system.
    /// Self-contained with optional integration points for other modules.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStatsModule", menuName = "MiST/Player Modules/Player Stats", order = 1)]
    public class PlayerStatsModule : PlayerModule
    {
        #region Configuration Fields

        [Header("Health System")]
        [Tooltip("Maximum health at level 1")]
        [SerializeField] private float baseMaxHealth = 100f;
        [Tooltip("Health regenerated per second")]
        [SerializeField] private float healthRegenRate = 5f;
        [Tooltip("Seconds to wait after taking damage before regeneration starts")]
        [SerializeField] private float healthRegenDelay = 5f;

        [Header("Stamina System")]
        [Tooltip("Maximum stamina at level 1")]
        [SerializeField] private float baseMaxStamina = 100f;
        [Tooltip("Stamina regenerated per second")]
        [SerializeField] private float staminaRegenRate = 20f;
        [Tooltip("Seconds to wait after using stamina before regeneration starts")]
        [SerializeField] private float staminaRegenDelay = 2f;

        [Header("Mana System")]
        [Tooltip("Maximum mana at level 1")]
        [SerializeField] private float baseMaxMana = 100f;
        [Tooltip("Mana regenerated per second")]
        [SerializeField] private float manaRegenRate = 10f;
        [Tooltip("Seconds to wait after using mana before regeneration starts")]
        [SerializeField] private float manaRegenDelay = 3f;

        [Header("Progression System")]
        [Tooltip("Starting level")]
        [SerializeField] private int startingLevel = 1;
        [Tooltip("XP required curve - X axis = level, Y axis = XP required for that level")]
        [SerializeField] private AnimationCurve xpRequiredCurve = AnimationCurve.Linear(1, 100, 20, 2000);
        [Tooltip("Fully regenerate health/stamina/mana when leveling up")]
        [SerializeField] private bool regenerateOnLevelUp = true;
        [Tooltip("Percentage increase to max stats per level (e.g., 0.1 = 10% increase)")]
        [SerializeField] private float statIncreasePerLevel = 0.1f;

        #endregion

        #region Runtime State

        private PlayerStateMachine cachedStateMachine;

        // Health
        private LazyValue<float> currentHealth;
        private float lastDamageTime = -999f;

        // Stamina
        private LazyValue<float> currentStamina;
        private float lastStaminaUseTime = -999f;

        // Mana
        private LazyValue<float> currentMana;
        private float lastManaUseTime = -999f;

        // Progression
        private LazyValue<int> currentLevel;
        private LazyValue<float> currentExperience;

        #endregion

        #region Properties

        // Health Properties
        public float CurrentHealth => currentHealth?.value ?? MaxHealth;
        public float MaxHealth => baseMaxHealth * (1 + (CurrentLevel - 1) * statIncreasePerLevel);
        public bool IsDead { get; private set; }

        // Stamina Properties
        public float CurrentStamina => currentStamina?.value ?? MaxStamina;
        public float MaxStamina => baseMaxStamina * (1 + (CurrentLevel - 1) * statIncreasePerLevel);

        // Mana Properties
        public float CurrentMana => currentMana?.value ?? MaxMana;
        public float MaxMana => baseMaxMana * (1 + (CurrentLevel - 1) * statIncreasePerLevel);

        // Progression Properties
        public int CurrentLevel => currentLevel?.value ?? startingLevel;
        public float CurrentXP => currentExperience?.value ?? 0f;
        public float ExperienceToNextLevel => xpRequiredCurve.Evaluate(CurrentLevel + 1);

        #endregion

        #region Events

        // Health Events
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;

        // Stamina Events
        public event Action<float> OnStaminaChanged;
        public event Action OnStaminaDepleted;

        // Mana Events
        public event Action<float> OnManaChanged;

        // Progression Events
        public event Action OnLevelUp;
        public event Action<float> OnExperienceGained;

        #endregion

        #region Module Lifecycle

        public override void Install(PlayerStateMachine sm)
        {
            cachedStateMachine = sm;

            // Initialize LazyValues
            currentHealth = new LazyValue<float>(() => MaxHealth);
            currentStamina = new LazyValue<float>(() => MaxStamina);
            currentMana = new LazyValue<float>(() => MaxMana);
            currentLevel = new LazyValue<int>(() => startingLevel);
            currentExperience = new LazyValue<float>(() => 0f);

            IsDead = false;

            // Start regeneration coroutines
            sm.StartCoroutine(HealthRegenCoroutine());
            sm.StartCoroutine(StaminaRegenCoroutine());
            sm.StartCoroutine(ManaRegenCoroutine());

            Debug.Log($"[PlayerStatsModule] Installed - HP: {MaxHealth}, Stamina: {MaxStamina}, Mana: {MaxMana}, Level: {CurrentLevel}");
        }

        #endregion

        #region Health System

        /// <summary>
        /// Deal damage to the player. Interrupts health regeneration.
        /// </summary>
        /// <param name="damage">Amount of damage to deal</param>
        public void DealDamage(float damage)
        {
            if (IsDead) return;

            float previousHealth = currentHealth.value;
            currentHealth.value = Mathf.Max(0, currentHealth.value - damage);
            lastDamageTime = Time.time;

            // Fire events
            OnHealthChanged?.Invoke(currentHealth.value);
            EventManager.TriggerEvent("Player.HealthChanged", currentHealth.value);

            Debug.Log($"[PlayerStatsModule] Took {damage} damage. HP: {currentHealth.value}/{MaxHealth}");

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

            Debug.Log($"[PlayerStatsModule] Healed {amount}. HP: {currentHealth.value}/{MaxHealth}");
        }

        /// <summary>
        /// Fully restore health to maximum.
        /// </summary>
        public void RestoreHealth()
        {
            Heal(MaxHealth);
        }

        private void HandleDeath()
        {
            IsDead = true;

            Debug.Log("[PlayerStatsModule] Player died!");

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

        #region Stamina System

        /// <summary>
        /// Attempt to use stamina. Returns false if insufficient stamina.
        /// </summary>
        /// <param name="amount">Amount of stamina to use</param>
        /// <returns>True if stamina was used, false if insufficient</returns>
        public bool UseStamina(float amount)
        {
            if (currentStamina.value < amount)
            {
                Debug.LogWarning($"[PlayerStatsModule] Insufficient stamina. Need {amount}, have {currentStamina.value}");
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
                Debug.Log("[PlayerStatsModule] Stamina depleted!");
            }

            return true;
        }

        /// <summary>
        /// Restore stamina by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to restore</param>
        public void RestoreStamina(float amount)
        {
            currentStamina.value = Mathf.Min(MaxStamina, currentStamina.value + amount);
            OnStaminaChanged?.Invoke(currentStamina.value);
        }

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

        #region Mana System

        /// <summary>
        /// Attempt to use mana. Returns false if insufficient mana.
        /// </summary>
        /// <param name="amount">Amount of mana to use</param>
        /// <returns>True if mana was used, false if insufficient</returns>
        public bool UseMana(float amount)
        {
            if (currentMana.value < amount)
            {
                Debug.LogWarning($"[PlayerStatsModule] Insufficient mana. Need {amount}, have {currentMana.value}");
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
        /// <param name="amount">Amount to restore</param>
        public void RestoreMana(float amount)
        {
            currentMana.value = Mathf.Min(MaxMana, currentMana.value + amount);
            OnManaChanged?.Invoke(currentMana.value);
            EventManager.TriggerEvent("Player.ManaChanged", currentMana.value);
        }

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

        #region Progression System

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

            Debug.Log($"[PlayerStatsModule] Gained {amount} XP. Total: {currentExperience.value}/{ExperienceToNextLevel}");

            // Check for level up
            while (currentExperience.value >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            // Deduct XP required for level
            currentExperience.value -= ExperienceToNextLevel;

            // Increase level
            currentLevel.value++;

            Debug.Log($"[PlayerStatsModule] Level Up! Now level {CurrentLevel}");

            // Regenerate resources if enabled
            if (regenerateOnLevelUp)
            {
                RestoreHealth();
                RestoreStamina(MaxStamina);
                RestoreMana(MaxMana);
                Debug.Log("[PlayerStatsModule] Resources fully restored on level up!");
            }

            // Fire events
            OnLevelUp?.Invoke();
            EventManager.TriggerEvent("Player.LevelUp", CurrentLevel);
        }

        #endregion
    }
}
