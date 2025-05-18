using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : BaseMenu
{
    [Header("Options Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button backButton;

    protected override void Awake()
    {
        // Explicitly set the menu ID to ensure it matches
        menuId = "OptionsMenu";

        // Configure menu properties
        menuType = MenuType.Additive;  // Options stack on top of pause menu
        shouldPauseGame = false;        // Don't change pause state
        handlesOwnInput = false;        // Use default input handling

        base.Awake();
        SetupButtons();
        SetupControls();
    }

    private void SetupButtons()
    {
        backButton.onClick.AddListener(OnBackClicked);

        // Set default selection for gamepad navigation
        defaultSelection = masterVolumeSlider;
    }

    private void SetupControls()
    {
        // Setup volume slider
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioListener.volume;
            masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // Setup fullscreen toggle
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        // Setup resolution dropdown
        if (resolutionDropdown != null)
        {
            SetupResolutionDropdown();
        }
    }

    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        Resolution[] resolutions = Screen.resolutions;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                resolutionDropdown.value = i;
            }
        }

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void OnBackClicked()
    {
        MenuManager.Instance.GoBack();
    }

    private void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
        // Trigger event for other systems that might need to know about volume changes
        EventManager.TriggerEvent("VolumeChanged");
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    private void OnResolutionChanged(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        PlayerPrefs.SetString("Resolution", $"{resolution.width}x{resolution.height}");
    }

    // Example of custom input handling for fine-tuning volume with gamepad
    public override void OnNavigationInput(Vector2 input)
    {
        // If volume slider is selected and we get horizontal input
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == masterVolumeSlider.gameObject)
        {
            if (Mathf.Abs(input.x) > 0.1f)
            {
                float newValue = masterVolumeSlider.value + (input.x * 0.01f);
                masterVolumeSlider.value = Mathf.Clamp01(newValue);
                return; // Don't call base implementation
            }
        }

        // Default navigation for other elements
        base.OnNavigationInput(input);
    }
}