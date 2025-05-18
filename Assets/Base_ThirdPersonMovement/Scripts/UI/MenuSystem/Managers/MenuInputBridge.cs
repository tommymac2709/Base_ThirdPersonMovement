using UnityEngine;

public class MenuInputBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;

    private void Start()
    {
        // Subscribe to InputReader events and forward them to EventManager
        if (inputReader != null)
        {
            inputReader.CancelWindowEvent += OnCancelInput;
            inputReader.SubmitEvent += OnSubmitInput;
            inputReader.NavigateEvent += OnNavigateInput;
            inputReader.PauseEvent += OnPauseInput;
        }

        // Subscribe to game state events
        EventManager.StartListening("GamePaused", OnGamePaused);
        EventManager.StartListening("GameResumed", OnGameResumed);
    }

    private void OnDestroy()
    {
        // Clean up subscriptions
        if (inputReader != null)
        {
            inputReader.CancelWindowEvent -= OnCancelInput;
            inputReader.SubmitEvent -= OnSubmitInput;
            inputReader.NavigateEvent -= OnNavigateInput;
            inputReader.PauseEvent -= OnPauseInput;
        }

        EventManager.StopListening("GamePaused", OnGamePaused);
        EventManager.StopListening("GameResumed", OnGameResumed);
    }

    private void OnCancelInput()
    {
        Debug.Log($"MenuInputBridge: Cancel input received. Menu count: {MenuManager.Instance?.MenuCount}");

        // Forward cancel input to menu system
        EventManager.TriggerEvent("MenuBack");
    }

    private void OnSubmitInput()
    {
        // Forward submit input to menu system
        EventManager.TriggerEvent("MenuConfirm");
    }

    private void OnNavigateInput(Vector2 navigation)
    {
        // Forward navigation input to menu system
        EventManager.TriggerEvent("MenuNavigate");
    }

    private void OnPauseInput()
    {
        Debug.Log($"MenuInputBridge: Pause input received. Menu count: {MenuManager.Instance?.MenuCount}");

        // For pause input, also forward to cancel handling since they should behave the same
        EventManager.TriggerEvent("MenuPauseToggle");
    }

    private void OnGamePaused()
    {
        Debug.Log("Game paused - disabling player controls");
        // Disable player controls when game is paused
        inputReader?.DisableControls();
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void OnGameResumed()
    {
        Debug.Log("Game resumed - enabling player controls");
        // Enable player controls when game is resumed
        inputReader?.EnableControls();
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}