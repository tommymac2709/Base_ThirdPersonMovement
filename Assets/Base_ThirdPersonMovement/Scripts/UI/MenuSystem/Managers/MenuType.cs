// MenuType.cs
public enum MenuType
{
    Overlay,    // Appears on top, doesn't close previous (notifications, HUD overlays)
    Replace,    // Replaces current menu completely (main menu to options)
    Additive,   // Adds to stack, previous menu hidden (pause -> options)
    Modal       // Blocks all interaction with previous menus (confirmation dialogs)
}