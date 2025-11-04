using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Stats;

namespace MistInteractive.ThirdPersonMovement.UI
{
    /// <summary>
    /// UI controller for displaying player stats (Health, Stamina, Mana, XP, Level).
    /// Automatically finds stat modules and subscribes to events.
    /// Gracefully handles missing modules - only displays UI for installed modules.
    /// </summary>
    public class StatsUIController : MonoBehaviour
    {
        [Header("Health UI")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Stamina UI")]
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private TextMeshProUGUI staminaText;

        [Header("Mana UI")]
        [SerializeField] private Slider manaSlider;
        [SerializeField] private TextMeshProUGUI manaText;

        [Header("Progression UI")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI xpText;

        [Header("References")]
        [SerializeField] private PlayerStateMachine playerStateMachine;

        private PlayerHealthModule healthModule;
        private PlayerStaminaModule staminaModule;
        private PlayerManaModule manaModule;
        private PlayerProgressionModule progressionModule;

        private void Start()
        {
            // Find PlayerStateMachine if not assigned
            if (playerStateMachine == null)
            {
                playerStateMachine = FindFirstObjectByType<PlayerStateMachine>();
                if (playerStateMachine == null)
                {
                    Debug.LogError("[StatsUIController] Could not find PlayerStateMachine in scene!");
                    enabled = false;
                    return;
                }
            }

            // Get all stat modules (null checks allow graceful degradation)
            healthModule = playerStateMachine.GetModule<PlayerHealthModule>();
            staminaModule = playerStateMachine.GetModule<PlayerStaminaModule>();
            manaModule = playerStateMachine.GetModule<PlayerManaModule>();
            progressionModule = playerStateMachine.GetModule<PlayerProgressionModule>();

            // Subscribe to events (with null checks)
            if (healthModule != null)
            {
                healthModule.OnHealthChanged += UpdateHealthUI;
            }

            if (staminaModule != null)
            {
                staminaModule.OnStaminaChanged += UpdateStaminaUI;
            }

            if (manaModule != null)
            {
                manaModule.OnManaChanged += UpdateManaUI;
            }

            if (progressionModule != null)
            {
                progressionModule.OnLevelUp += UpdateLevelUI;
                progressionModule.OnExperienceGained += UpdateXPUI;
            }

            // Initial update
            UpdateAllUI();

            // Log which modules were found
            Debug.Log($"[StatsUIController] Initialized - Health: {healthModule != null}, Stamina: {staminaModule != null}, Mana: {manaModule != null}, Progression: {progressionModule != null}");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (healthModule != null)
            {
                healthModule.OnHealthChanged -= UpdateHealthUI;
            }

            if (staminaModule != null)
            {
                staminaModule.OnStaminaChanged -= UpdateStaminaUI;
            }

            if (manaModule != null)
            {
                manaModule.OnManaChanged -= UpdateManaUI;
            }

            if (progressionModule != null)
            {
                progressionModule.OnLevelUp -= UpdateLevelUI;
                progressionModule.OnExperienceGained -= UpdateXPUI;
            }
        }

        private void UpdateAllUI()
        {
            if (healthModule != null)
                UpdateHealthUI(healthModule.CurrentHealth);

            if (staminaModule != null)
                UpdateStaminaUI(staminaModule.CurrentStamina);

            if (manaModule != null)
                UpdateManaUI(manaModule.CurrentMana);

            if (progressionModule != null)
            {
                UpdateLevelUI();
                UpdateXPUI(0); // Trigger XP display update
            }
        }

        private void UpdateHealthUI(float currentHealth)
        {
            if (healthModule == null) return;

            if (healthSlider != null)
            {
                healthSlider.maxValue = healthModule.MaxHealth;
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(healthModule.MaxHealth)}";
            }
        }

        private void UpdateStaminaUI(float currentStamina)
        {
            if (staminaModule == null) return;

            if (staminaSlider != null)
            {
                staminaSlider.maxValue = staminaModule.MaxStamina;
                staminaSlider.value = currentStamina;
            }

            if (staminaText != null)
            {
                staminaText.text = $"{Mathf.CeilToInt(currentStamina)}/{Mathf.CeilToInt(staminaModule.MaxStamina)}";
            }
        }

        private void UpdateManaUI(float currentMana)
        {
            if (manaModule == null) return;

            if (manaSlider != null)
            {
                manaSlider.maxValue = manaModule.MaxMana;
                manaSlider.value = currentMana;
            }

            if (manaText != null)
            {
                manaText.text = $"{Mathf.CeilToInt(currentMana)}/{Mathf.CeilToInt(manaModule.MaxMana)}";
            }
        }

        private void UpdateLevelUI()
        {
            if (progressionModule == null) return;

            if (levelText != null)
            {
                levelText.text = progressionModule.CurrentLevel.ToString();
            }
        }

        private void UpdateXPUI(float xpGained)
        {
            if (progressionModule == null) return;

            if (xpSlider != null)
            {
                xpSlider.maxValue = progressionModule.ExperienceToNextLevel;
                xpSlider.value = progressionModule.CurrentXP;
            }

            if (xpText != null)
            {
                xpText.text = $"{Mathf.FloorToInt(progressionModule.CurrentXP)}/{Mathf.FloorToInt(progressionModule.ExperienceToNextLevel)} XP";
            }
        }

        [ContextMenu("Deal 25 Damage")]
        void TestDamage()
        {
            healthModule?.TakeDamage(25f);
        }

        [ContextMenu("Heal 50 HP")]
        void TestHeal()
        {
            healthModule?.Heal(50f);
        }

        [ContextMenu("Gain 100 XP")]
        void TestXP()
        {
            progressionModule?.GainExperience(100f);
        }

        [ContextMenu("Use 30 Stamina")]
        void TestStamina()
        {
            staminaModule?.Use(30f);
        }

        [ContextMenu("Use 20 Mana")]
        void TestMana()
        {
            manaModule?.Use(20f);
        }
    }
}
