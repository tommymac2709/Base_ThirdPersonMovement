// Base class for managing UI windows (pause menus, etc.), handling time scaling and input.

using System.Collections.Generic;
using UnityEngine;

public abstract class WindowController : MonoBehaviour
{
    protected static HashSet<WindowController> activeWindows = new HashSet<WindowController>();

    public static event System.Action OnAnyWindowOpened;
    public static event System.Action OnAllWindowsClosed;

    protected InputReader InputReader;

    protected virtual void Awake()
    {
        InputReader = GameObject.FindWithTag("Player").GetComponent<InputReader>();
        Subscribe();
    }

    protected virtual void OnDestroy()
    {
        Unsubscribe();
    }

    void OnEnable()
    {
        activeWindows.Add(this);
        Time.timeScale = 0.0f;
        OnAnyWindowOpened?.Invoke();
    }

    void OnDisable()
    {
        activeWindows.Remove(this);
        if (activeWindows.Count == 0)
        {
            Time.timeScale = 1.0f;
            OnAllWindowsClosed?.Invoke();
        }
    }

    /// <summary>
    /// Override this method to subscribe to relevant events for the derived window controller.
    /// </summary>
    protected abstract void Subscribe();

    /// <summary>
    /// Override this method to unsubscribe from events for the derived window controller.
    /// </summary>
    protected abstract void Unsubscribe();

    /// <summary>
    /// Closes this window.
    /// </summary>
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggles the window's active state.
    /// </summary>
    protected void ToggleWindow()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
