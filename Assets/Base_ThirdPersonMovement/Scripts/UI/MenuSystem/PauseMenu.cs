using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : BaseMenu
{
    [Header("Pause Menu Buttons")]
    [SerializeField] private UnityEngine.UI.Button resumeButton;
    [SerializeField] private UnityEngine.UI.Button optionsButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;

    protected override void Awake()
    {
        base.Awake();
        SetupButtons();
    }

    private void SetupButtons()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnResumeClicked()
    {
        MenuManager.Instance.ResumeGame();
    }

    private void OnOptionsClicked()
    {
        MenuManager.Instance.OpenMenu("OptionsMenu");
    }

    private void OnQuitClicked()
    {
        MenuManager.Instance.QuitGame();
    }

   

    public override void OnMenuOpened()
    {
        // Pause game when this menu opens
        Time.timeScale = 0f;
        AudioListener.pause = true;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            eventSystem.enabled = true;
        }
    }

    public override void OnMenuClosed()
    {
        // Resume game when this menu closes (if no other menus are open)
        if (MenuManager.Instance.MenuCount == 0)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}