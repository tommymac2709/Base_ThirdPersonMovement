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

    private void Update()
    {
        HandleInput();
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

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuStack.Count == 0)
            {
                OpenMenu("PauseMenu");
            }
            else
            {
                GoBack();
            }
        }
    }

    public void OpenMenu(string menuId)
    {
        if (!menuRegistry.ContainsKey(menuId))
        {
            Debug.LogError($"Menu with ID '{menuId}' not found!");
            return;
        }

        // Hide current top menu if exists
        if (menuStack.Count > 0)
        {
            menuStack.Peek().Hide();
        }

        // Show new menu
        IMenu menu = menuRegistry[menuId];
        menu.Show();
        menuStack.Push(menu);
    }

    public void GoBack()
    {
        if (menuStack.Count > 0)
        {
            // Hide current menu
            IMenu currentMenu = menuStack.Pop();
            currentMenu.Hide();

            // Show previous menu if exists
            if (menuStack.Count > 0)
            {
                menuStack.Peek().Show();
            }
        }
    }

    public void CloseAllMenus()
    {
        while (menuStack.Count > 0)
        {
            IMenu menu = menuStack.Pop();
            menu.Hide();
        }
    }

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