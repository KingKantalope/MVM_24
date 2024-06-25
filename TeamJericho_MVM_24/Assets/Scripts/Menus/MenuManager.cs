using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Menu MainMenu;
    [SerializeField] private List<Menu> Menus;
    [SerializeField] public readonly float fadeSpeed = 2.0f;

    public readonly Vector2 BackwardOffset = new Vector2(-50f, 0f);
    public readonly Vector2 CenterPos = new Vector2(0f,0f);
    public readonly Vector2 ForwardOffset = new Vector2(50f, 0f);

    // change this for when menus need to be animated
    public void ChangeMenu(string menu)
    {
        Menu nextMenu = null;

        foreach (var m in Menus)
        {
            if (menu == m.MenuName)
            {
                nextMenu = m;
            }
        }
    }

    public void OpenOptions()
    {
        // disable interactions with current menu

        // fade out current menu's non-background

        // fade in options menu

        // enable interactions with options
    }

    public void CloseOptions()
    {
        // disable interactions with options

        // fade out options

        // fade in current menu's non-background

        // enable interactions with current menu
    }
}
