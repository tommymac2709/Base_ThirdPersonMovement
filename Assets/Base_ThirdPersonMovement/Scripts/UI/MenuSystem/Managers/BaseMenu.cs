using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class BaseMenu : MonoBehaviour, IMenu
{
    [Header("Menu Configuration")]
    [SerializeField] protected string menuId;
    [SerializeField] protected MenuType menuType;
    [SerializeField] protected GameObject menuPanel;
    [SerializeField] protected bool handlesOwnInput = false;
    [SerializeField] protected bool shouldPauseGame = false;

    [Header("Navigation")]
    [SerializeField] protected Selectable defaultSelection;
    [SerializeField] protected Selectable lastSelectedElement;

    // Interface properties
    public string MenuId => menuId;
    public MenuType MenuType => menuType;
    public bool IsActive => menuPanel != null && menuPanel.activeInHierarchy;
    public bool HandlesOwnInput => handlesOwnInput;
    public bool ShouldPauseGame => shouldPauseGame;

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(menuId))
            menuId = GetType().Name;

        // Auto-detect pause behavior based on menu type
        if (shouldPauseGame == false)
        {
            shouldPauseGame = menuType == MenuType.Modal || menuType == MenuType.Additive;
        }
    }

    protected virtual void OnEnable()
    {
        // Subscribe to input events when menu is enabled
        SubscribeToInputEvents();
    }

    protected virtual void OnDisable()
    {
        // Unsubscribe from input events when menu is disabled
        UnsubscribeFromInputEvents();
    }

    protected virtual void SubscribeToInputEvents()
    {
        EventManager.StartListening("MenuNavigate", OnNavigationInputEvent);
        EventManager.StartListening("MenuConfirm", OnConfirmInputEvent);
        EventManager.StartListening("MenuCancel", OnCancelInputEvent);
    }

    protected virtual void UnsubscribeFromInputEvents()
    {
        EventManager.StopListening("MenuNavigate", OnNavigationInputEvent);
        EventManager.StopListening("MenuConfirm", OnConfirmInputEvent);
        EventManager.StopListening("MenuCancel", OnCancelInputEvent);
    }

    // Event handlers that dispatch to virtual methods
    private void OnNavigationInputEvent()
    {
        OnNavigationInput(Vector2.zero); // Navigation will be handled by Unity's EventSystem
    }

    private void OnConfirmInputEvent()
    {
        if (IsActive && (HandlesOwnInput || MenuManager.Instance?.GetCurrentMenu() == this))
            OnConfirmInput();
    }

    private void OnCancelInputEvent()
    {
        if (IsActive && (HandlesOwnInput || MenuManager.Instance?.GetCurrentMenu() == this))
            OnCancelInput();
    }

    public virtual void Show()
    {
        menuPanel.SetActive(true);
        HandleGamepadNavigation();
        OnMenuOpened();
    }

    public virtual void Hide()
    {
        SaveCurrentSelection();
        menuPanel.SetActive(false);
        OnMenuClosed();
    }

    public virtual void OnMenuOpened()
    {
        // Trigger events for game state changes
        if (ShouldPauseGame)
        {
            EventManager.TriggerEvent("GamePaused");
        }

        EventManager.TriggerEvent($"Menu_{MenuId}_Opened");
    }

    public virtual void OnMenuClosed()
    {
        // Resume game if this was the last menu and it was pausing
        if (ShouldPauseGame && MenuManager.Instance.MenuCount == 0)
        {
            EventManager.TriggerEvent("GameResumed");
        }

        EventManager.TriggerEvent($"Menu_{MenuId}_Closed");
    }

    // Input handling - can be overridden by derived classes
    public virtual void OnNavigationInput(Vector2 input)
    {
        // Default: let Unity handle gamepad navigation through EventSystem
        HandleGamepadNavigation();
    }

    public virtual void OnConfirmInput()
    {
        // Default: simulate click on selected element
        var selected = EventSystem.current?.currentSelectedGameObject;
        if (selected != null)
        {
            var button = selected.GetComponent<Button>();
            button?.onClick.Invoke();
        }
    }

    public virtual void OnCancelInput()
    {
        // Default: go back in menu system
        MenuManager.Instance?.GoBack();
    }

    public virtual void OnMenuSpecificInput(string actionName)
    {
        // Override in derived classes for menu-specific inputs
    }

    protected virtual void HandleGamepadNavigation()
    {
        // Set default selection for gamepad navigation
        if (EventSystem.current != null)
        {
            if (lastSelectedElement != null && lastSelectedElement.isActiveAndEnabled)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedElement.gameObject);
            }
            else if (defaultSelection != null && defaultSelection.isActiveAndEnabled)
            {
                EventSystem.current.SetSelectedGameObject(defaultSelection.gameObject);
            }
            else
            {
                // Find first selectable element
                Selectable firstSelectable = menuPanel.GetComponentInChildren<Selectable>();
                if (firstSelectable != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
                }
            }
        }
    }

    protected virtual void SaveCurrentSelection()
    {
        if (EventSystem.current?.currentSelectedGameObject != null)
        {
            lastSelectedElement = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
        }
    }
}