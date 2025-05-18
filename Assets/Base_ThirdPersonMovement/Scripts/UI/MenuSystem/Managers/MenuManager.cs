using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Menu References")]
    [SerializeField] private List<BaseMenu> allMenus = new List<BaseMenu>();

    private Stack<IMenu> menuStack = new Stack<IMenu>();
    private Dictionary<string, IMenu> menuRegistry = new Dictionary<string, IMenu>();

    public int MenuCount => menuStack.Count;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMenus();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to events
        EventManager.StartListening("MenuBack", OnBackInput);
        EventManager.StartListening("MenuPauseToggle", OnPauseToggleInput);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventManager.StopListening("MenuBack", OnBackInput);
        EventManager.StopListening("MenuPauseToggle", OnPauseToggleInput);
    }

    private void InitializeMenus()
    {
        // Register all menus
        foreach (var menu in allMenus)
        {
            menuRegistry[menu.MenuId] = menu;
            menu.Hide(); // Start with all menus hidden
        }
    }

    private void OnBackInput()
    {
        // this is only ever your “go back one level” logic:
        if (menuStack.Count > 0)
            GoBack();
    }

    private void OnPauseToggleInput()
    {
        // your old OnGlobalCancelInput logic, but only for Pause toggles:
        if (menuStack.Count == 0)
        {
            OpenMenu("PauseMenu");
        }
        else
        {
            var current = GetCurrentMenu();
            if (current?.MenuId == "PauseMenu")
                ResumeGame();
            else
                GoBack();
        }
    }

    public void OpenMenu(string menuId)
    {
        if (!menuRegistry.ContainsKey(menuId))
        {
            Debug.LogError($"Menu with ID '{menuId}' not found!");
            return;
        }

        IMenu newMenu = menuRegistry[menuId];
        IMenu currentMenu = menuStack.Count > 0 ? menuStack.Peek() : null;

        // Handle different menu types
        switch (newMenu.MenuType)
        {
            case MenuType.Replace:
                // Close all existing menus and show new one
                CloseAllMenus();
                break;

            case MenuType.Modal:
            case MenuType.Additive:
                // Hide current menu but keep in stack
                currentMenu?.Hide();
                break;

            case MenuType.Overlay:
                // Don't hide current menu, just add on top
                break;
        }

        // Show new menu and add to stack
        newMenu.Show();
        menuStack.Push(newMenu);

        // Trigger events
        EventManager.TriggerEvent("MenuStackChanged");
        EventManager.TriggerEvent($"Menu_{menuId}_Opened");
    }

    public void GoBack()
    {
        if (menuStack.Count > 0)
        {
            // Hide and remove current menu
            IMenu currentMenu = menuStack.Pop();
            string menuId = currentMenu.MenuId;
            currentMenu.Hide();

            // Show previous menu if exists and menu type allows it
            if (menuStack.Count > 0)
            {
                IMenu previousMenu = menuStack.Peek();
                if (currentMenu.MenuType != MenuType.Overlay)
                {
                    previousMenu.Show();
                }
            }

            // Trigger events
            EventManager.TriggerEvent("MenuStackChanged");
            EventManager.TriggerEvent($"Menu_{menuId}_Closed");
        }
    }

    public void CloseAllMenus()
    {
        while (menuStack.Count > 0)
        {
            IMenu menu = menuStack.Pop();
            menu.Hide();
        }

        EventManager.TriggerEvent("MenuStackChanged");
        EventManager.TriggerEvent("AllMenusClosed");
    }

    public IMenu GetCurrentMenu()
    {
        return menuStack.Count > 0 ? menuStack.Peek() : null;
    }

    public bool HasMenuOfType(MenuType menuType)
    {
        foreach (var menu in menuStack)
        {
            if (menu.MenuType == menuType)
                return true;
        }
        return false;
    }

    // Convenience methods
    public void ResumeGame()
    {
        CloseAllMenus();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}