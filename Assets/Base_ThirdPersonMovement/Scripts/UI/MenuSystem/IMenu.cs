using System.Threading.Tasks;
using UnityEngine;

public interface IMenu
{
    string MenuId { get; }
    bool IsActive { get; }

    void Show();
    void Hide();
    void OnMenuOpened();
    void OnMenuClosed();
}