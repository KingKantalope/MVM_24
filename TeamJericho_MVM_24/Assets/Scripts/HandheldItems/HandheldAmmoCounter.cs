using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandheldAmmoCounter : HandheldDisplay
{
    [Header("additional stuff")]
    [SerializeField] private Color unavailableColor;
    [SerializeField] private Color warningUnavailableColor;
    [SerializeField] private Image resourceBar;
    [SerializeField] private Image resourceBarBackground;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SetResourceAmount(int available, int maxAvailable, float lowThreshold)
    {
        float barAmount = (float)available / (float)maxAvailable;

        resourceBar.fillAmount = barAmount;

        if (barAmount > lowThreshold)
        {
            // set color to defaultColor
            resourceBar.color = defaultColor;
            resourceBarBackground.color = unavailableColor;

            // stop warning indicator animation
            warningHazeAnimator.SetBool("Active", false);
        }
        else
        {
            // set color to warningColor
            resourceBar.color = warningColor;
            resourceBarBackground.color = warningUnavailableColor;

            // start warning indicator animation
            warningHazeAnimator.SetBool("Active", true);
        }
    }

    public override void SetResourceAmount(int available, int maxAvailable, float lowThreshold, int reserves, int reservesThreshold)
    {
        reserveResourceText.text = reserves.ToString();

        float barAmount = (float)available / (float)maxAvailable;

        resourceBar.fillAmount = barAmount;

        if (reserves > reservesThreshold) resourceBackground.color = backgroundColor;
        else resourceBackground.color = warningColor;

        if (barAmount > lowThreshold)
        {
            // set color to defaultColor
            resourceBackground.color = backgroundColor;
            resourceBar.color = defaultColor;
            resourceBarBackground.color = unavailableColor;

            // stop warning indicator animation
            warningHazeAnimator.SetBool("Active", false);
        }
        else
        {
            // set color to warningColor
            resourceBackground.color = warningColor;
            resourceBackground.color = warningColor;
            resourceBar.color = warningColor;
            resourceBarBackground.color = warningUnavailableColor;

            // start warning indicator animation
            warningHazeAnimator.SetBool("Active", true);
        }
    }
}
