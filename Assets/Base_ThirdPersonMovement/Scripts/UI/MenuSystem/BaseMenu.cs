using UnityEngine;

public abstract class BaseMenu : MonoBehaviour, IMenu
{
    [SerializeField] protected string menuId;
    [SerializeField] protected GameObject menuPanel;

    public string MenuId => menuId;
    public bool IsActive => menuPanel.activeInHierarchy;

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(menuId))
            menuId = GetType().Name;
    }

    public virtual void Show()
    {
        menuPanel.SetActive(true);
        OnMenuOpened();
    }

    public virtual void Hide()
    {
        menuPanel.SetActive(false);
        OnMenuClosed();
    }

    public virtual void OnMenuOpened()
    {
        // Override in derived classes for custom behavior
    }

    public virtual void OnMenuClosed()
    {
        // Override in derived classes for custom behavior
    }
}