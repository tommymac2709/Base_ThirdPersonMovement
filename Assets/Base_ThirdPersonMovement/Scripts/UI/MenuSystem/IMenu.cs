using UnityEngine;

public interface IMenu
{
    string MenuId { get; }
    MenuType MenuType { get; }
    bool IsActive { get; }
    bool HandlesOwnInput { get; }
    bool ShouldPauseGame { get; }

    void Show();
    void Hide();
    void OnMenuOpened();
    void OnMenuClosed();

    // Input handling
    void OnNavigationInput(Vector2 input);
    void OnConfirmInput();
    void OnCancelInput();
    void OnMenuSpecificInput(string actionName);
}