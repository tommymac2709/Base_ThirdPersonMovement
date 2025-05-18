using UnityEngine;

public class PauseMenu : BaseMenu
{
    [Header("Pause Menu Buttons")]
    [SerializeField] private UnityEngine.UI.Button resumeButton;
    [SerializeField] private UnityEngine.UI.Button optionsButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;

    protected override void Awake()
    {
        // Explicitly set the menu ID to ensure it matches
        menuId = "PauseMenu";

        // Configure menu properties
        menuType = MenuType.Additive;  // Pause menu goes on stack
        shouldPauseGame = true;         // Pause the game when shown
        handlesOwnInput = false;        // Use default input handling

        base.Awake();
        SetupButtons();
    }

    private void SetupButtons()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // Set default selection for gamepad navigation
        defaultSelection = resumeButton;
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

    // Optional: Custom input handling example
    public override void OnMenuSpecificInput(string actionName)
    {
        switch (actionName)
        {
            case "QuickResume":
                OnResumeClicked();
                break;
            case "QuickQuit":
                OnQuitClicked();
                break;
        }
    }
}