using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Stats;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

namespace MistInteractive.ThirdPersonMovement.UI
{
    /// <summary>
    /// UI controller for displaying player stats (Health, Stamina, Mana, XP, Level).
    /// Automatically finds PlayerStatsModule and subscribes to events.
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

        private PlayerStatsModule statsModule;

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

            // Get PlayerStatsModule
            statsModule = playerStateMachine.GetModule<PlayerStatsModule>();
            if (statsModule == null)
            {
                Debug.LogError("[StatsUIController] PlayerStateMachine does not have a PlayerStatsModule!");
                enabled = false;
                return;
            }

            // Subscribe to events
            statsModule.OnHealthChanged += UpdateHealthUI;
            statsModule.OnStaminaChanged += UpdateStaminaUI;
            statsModule.OnManaChanged += UpdateManaUI;
            statsModule.OnLevelUp += UpdateLevelUI;
            statsModule.OnExperienceGained += UpdateXPUI;

            // Initial update
            UpdateAllUI();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (statsModule != null)
            {
                statsModule.OnHealthChanged -= UpdateHealthUI;
                statsModule.OnStaminaChanged -= UpdateStaminaUI;
                statsModule.OnManaChanged -= UpdateManaUI;
                statsModule.OnLevelUp -= UpdateLevelUI;
                statsModule.OnExperienceGained -= UpdateXPUI;
            }
        }

        private void UpdateAllUI()
        {
            UpdateHealthUI(statsModule.CurrentHealth);
            UpdateStaminaUI(statsModule.CurrentStamina);
            UpdateManaUI(statsModule.CurrentMana);
            UpdateLevelUI();
            UpdateXPUI(0); // Trigger XP display update
        }

        private void UpdateHealthUI(float currentHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = statsModule.MaxHealth;
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(statsModule.MaxHealth)}";
            }
        }

        private void UpdateStaminaUI(float currentStamina)
        {
            if (staminaSlider != null)
            {
                staminaSlider.maxValue = statsModule.MaxStamina;
                staminaSlider.value = currentStamina;
            }

            if (staminaText != null)
            {
                staminaText.text = $"{Mathf.CeilToInt(currentStamina)}/{Mathf.CeilToInt(statsModule.MaxStamina)}";
            }
        }

        private void UpdateManaUI(float currentMana)
        {
            if (manaSlider != null)
            {
                manaSlider.maxValue = statsModule.MaxMana;
                manaSlider.value = currentMana;
            }

            if (manaText != null)
            {
                manaText.text = $"{Mathf.CeilToInt(currentMana)}/{Mathf.CeilToInt(statsModule.MaxMana)}";
            }
        }

        private void UpdateLevelUI()
        {
            if (levelText != null)
            {
                levelText.text = statsModule.CurrentLevel.ToString();
            }
        }

        private void UpdateXPUI(float xpGained)
        {
            if (xpSlider != null)
            {
                xpSlider.maxValue = statsModule.ExperienceToNextLevel;
                xpSlider.value = statsModule.CurrentXP;
            }

            if (xpText != null)
            {
                xpText.text = $"{Mathf.FloorToInt(statsModule.CurrentXP)}/{Mathf.FloorToInt(statsModule.ExperienceToNextLevel)} XP";
            }
        }

        [ContextMenu("Deal 25 Damage")]
        void TestDamage()
        {
            statsModule?.DealDamage(25f);
        }

        [ContextMenu("Heal 50 HP")]
        void TestHeal()
        {
            statsModule?.Heal(50f);
        }

        [ContextMenu("Gain 100 XP")]
        void TestXP()
        {
            statsModule?.GainExperience(100f);
        }

        [ContextMenu("Use 30 Stamina")]
        void TestStamina()
        {
            statsModule?.UseStamina(30f);
        }
    }
}
