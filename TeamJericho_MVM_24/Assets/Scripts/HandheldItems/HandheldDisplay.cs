using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandheldDisplay : MonoBehaviour
{
    [Header("Resource Text")]
    [SerializeField] protected Text currentResourceText;
    [SerializeField] protected Text reserveResourceText;
    [SerializeField] protected Image resourceDivider;
    [SerializeField] protected Image infiniteAmmo;
    [SerializeField] protected Color defaultColor;
    protected int maxResource;

    [Header("Low Resource Indicator")]
    [SerializeField] protected Animator warningHazeAnimator;
    [SerializeField] protected Color warningColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void OnAttachedDisplay(Color HUDColor)
    {
        // defaultColor = HUDColor;
    }

    public virtual void SetResourceAmount(int available, int maxAvailable, float lowThreshold)
    {
        currentResourceText.text = available.ToString();
        reserveResourceText.text = maxAvailable.ToString();

        if ((available / maxAvailable) <= lowThreshold)
        {
            // set color to warningColor
            currentResourceText.color = warningColor;
            reserveResourceText.color = warningColor;
            resourceDivider.color = warningColor;

            // start warning indicator animation
            warningHazeAnimator.SetBool("Active", true);
        }
        else
        {
            // set color to defaultColor
            currentResourceText.color = warningColor;
            reserveResourceText.color = warningColor;
            resourceDivider.color = warningColor;

            // stop warning indicator animation
            warningHazeAnimator.SetBool("Active", false);
        }
    }

    public virtual void SetResourceAmount(int available, int maxAvailable, float lowThreshold, int reserves)
    {
        currentResourceText.text = available.ToString();
        reserveResourceText.text = reserves.ToString();

        if ((available / maxAvailable) <= lowThreshold)
        {
            // set color to warningColor
            currentResourceText.color = warningColor;
            reserveResourceText.color = warningColor;
            resourceDivider.color = warningColor;

            // start warning indicator animation
            warningHazeAnimator.SetBool("Active", true);
        }
        else
        {
            // set color to defaultColor
            currentResourceText.color = warningColor;
            reserveResourceText.color = warningColor;
            resourceDivider.color = warningColor;

            // stop warning indicator animation
            warningHazeAnimator.SetBool("Active", false);
        }
    }
}