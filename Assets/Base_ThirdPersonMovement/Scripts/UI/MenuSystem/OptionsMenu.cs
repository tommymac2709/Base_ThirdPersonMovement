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
        base.Awake();
        SetupButtons();
        SetupControls();
    }

    private void SetupButtons()
    {
        backButton.onClick.AddListener(OnBackClicked);
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
        // Could also trigger an event here for other audio systems
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void OnResolutionChanged(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}