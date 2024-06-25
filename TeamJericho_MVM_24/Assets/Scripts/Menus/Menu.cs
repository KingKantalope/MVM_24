using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private MenuManager Manager;

    public string MenuName;
    [SerializeField] private CanvasGroup FullCanvas;
    [SerializeField] private CanvasGroup Background;
    [SerializeField] private CanvasGroup MenuCanvas;

    [SerializeField] private RectTransform backgroundTransform;
    [SerializeField] private RectTransform MenuTransform;

    [SerializeField] private float maxAlphaBackground = 1f;

    private Vector2 initialPos;
    private Vector2 targetPos;
    private float targetAlphaBackground;
    private float targetAlphaMenu;

    /*
    How should the menus be animated? Does unity have a curve data type?
    Maybe just the camera and options menu moves? Not sure how that would change controls and such.

    Fade In: From wherever, move menu to central position and fade in. Do background and actual menu
    separately.

    Fade Out: From central position, move menu either back or forward. Only fade out background if
    opening submenu, always fade out actual menu.
    */

    private void Awake()
    {
        initialPos = backgroundTransform.anchoredPosition;
        targetPos = initialPos;
    }

    private void Update()
    {
        // fade in/out
        if (Background.alpha != targetAlphaBackground)
        {
            Background.alpha = Mathf.MoveTowards(Background.alpha, targetAlphaBackground, Time.deltaTime * Manager.fadeSpeed);
        }
        if (MenuCanvas.alpha != targetAlphaMenu)
        {
            MenuCanvas.alpha = Mathf.MoveTowards(Background.alpha, targetAlphaBackground, Time.deltaTime * Manager.fadeSpeed);
        }
        if (backgroundTransform.anchoredPosition != targetPos)
        {
            backgroundTransform.anchoredPosition =
                Vector2.MoveTowards(backgroundTransform.anchoredPosition, targetPos, Time.deltaTime * Manager.fadeSpeed);
        }
    }

    public void FadeIn()
    {
        // fade in menu to default position
        targetPos = Manager.CenterPos;
        targetAlphaBackground = maxAlphaBackground;
        targetAlphaMenu = 1f;
    }

    public void FadeOut(bool toSubMenu)
    {
        if (!toSubMenu)
        {
            targetPos = Manager.BackwardOffset;
        }
        else
        {
            targetPos = Manager.ForwardOffset;
            targetAlphaBackground = 0f;
        }

        targetAlphaMenu = 0f;
    }
}
