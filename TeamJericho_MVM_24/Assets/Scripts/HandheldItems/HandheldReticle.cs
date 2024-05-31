using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandheldReticle : MonoBehaviour
{
    private Color defaultColor;
    private Color enemyColor;
    private Color friendlyColor;
    private CanvasGroup ReticleGroup;

    [Header("Base Stuff")]
    [SerializeField] private Image Reticle;

    // Start is called before the first frame update
    void Start()
    {
        SetVisibility(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetVisibility(bool visible)
    {
        ReticleGroup.alpha = visible ? 1 : 0;
    }

    public void OnCreateReticle(Color Default, Color Enemy, Color Friendly)
    {
        // get defaults from PlayerHUD
        defaultColor = Default;
        enemyColor = Enemy;
        friendlyColor = Friendly;
    }

    public virtual void SetReticleColor(bool isEnemy, bool isFriendly)
    {
        if (isEnemy)
        {
            if (Reticle.color != enemyColor)
            {
                Reticle.color = enemyColor;
            }
        }
        else if (isFriendly)
        {
            if (Reticle.color != friendlyColor)
            {
                Reticle.color = friendlyColor;
            }
        }
        else
        {
            if (Reticle.color != defaultColor)
            {
                Reticle.color = defaultColor;
            }
        }
    }

    public virtual void SetReticleSpread(float spread)
    {
        // update for anything that needs visualized dynamic spread
    }
}
