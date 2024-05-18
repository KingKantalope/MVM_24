using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandheldAmmoCounter : HandheldDisplay
{
    [Header("additional stuff")]
    [SerializeField] private Color unavailableColor;
    [SerializeField] private Color warningUnavailableColor;
    [SerializeField] private Image[] ammoImages;

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
        for (int i = 0; i < maxAvailable; i++)
        {
            if (((float)available / (float)maxAvailable) > lowThreshold)
            {
                // set color to defaultColor
                resourceDivider.color = defaultColor;
                infiniteAmmo.color = defaultColor;

                // stop warning indicator animation
                // warningHazeAnimator.SetBool("Active", false);

                if (i + 1 <= available)
                {
                    ammoImages[i].color = defaultColor;
                }
                else
                {
                    ammoImages[i].color = unavailableColor;
                }
            }
            else
            {
                // set color to warningColor
                resourceDivider.color = warningColor;
                infiniteAmmo.color = warningColor;

                // start warning indicator animation
                // warningHazeAnimator.SetBool("Active", true);

                if (i + 1 <= available)
                {
                    ammoImages[i].color = warningColor;
                }
                else
                {
                    ammoImages[i].color = warningUnavailableColor;
                }
            }
        }
    }

    public override void SetResourceAmount(int available, int maxAvailable, float lowThreshold, int reserves)
    {
        reserveResourceText.text = reserves.ToString();

        for (int i = 0; i < maxAvailable; i++)
        {
            if (((float)available/ (float)maxAvailable) > lowThreshold)
            {
                // set color to defaultColor
                reserveResourceText.color = defaultColor;
                resourceDivider.color = defaultColor;

                // stop warning indicator animation
                // warningHazeAnimator.SetBool("Active", false);

                if (i + 1 <= available)
                {
                    ammoImages[i].color = defaultColor;
                }
                else
                {
                    ammoImages[i].color = unavailableColor;
                }
            }
            else
            {
                // set color to warningColor
                reserveResourceText.color = warningColor;
                resourceDivider.color = warningColor;

                // start warning indicator animation
                // warningHazeAnimator.SetBool("Active", true);

                if (i + 1 <= available)
                {
                    ammoImages[i].color = warningColor;
                }
                else
                {
                    ammoImages[i].color = warningUnavailableColor;
                }
            }
        }
    }
}
