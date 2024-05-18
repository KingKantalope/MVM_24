using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    // Handheld Stuff
    private HandheldSlot currentSlot;
    private HandheldSlot nextSlot;
    private GameObject weaponOneDisplay;
    private Image weaponOneImage;
    private bool hasWeaponOne;
    private GameObject weaponTwoDisplay;
    private Image weaponTwoImage;
    private bool hasWeaponTwo;
    private GameObject sidearmDisplay;
    private Image sidearmImage;
    private bool hasSidearm;
    private GameObject pickupDisplay;
    private Image pickupImage;
    private bool hasPickup;

    private float HUDOffset;

    [Header("General Details")]
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color friendlyColor;
    [SerializeField] private Color enemyColor;
    [SerializeField] private RectTransform MainHUDParent;
    [SerializeField] private RectTransform ReticleParent;
    [SerializeField] private RectTransform OtherHUDParent;
    [SerializeField] private RectTransform ammoCounterParent;
    [SerializeField] private Image critIndicator;
    [SerializeField] private Image activeHandheld;
    [SerializeField] private Image firstStowedHandheld;
    [SerializeField] private Image secondStowedHandheld;
    [SerializeField] private GameObject defaultDisplay;
    [SerializeField] private GameObject defaultReticle;

    // Start is called before the first frame update
    void Start()
    {
        // set default rotation
        // set default HUD position
    }

    // Update is called once per frame
    void Update()
    {
        // set hud centered on actual aim
    }

    public (RectTransform, RectTransform) OnAttachHandheldHUDFIXED()
    {
        return (ReticleParent, ammoCounterParent);
    }

    public void OnAttachHandheldHUD(IHandheldObject handheldObject, GameObject handheldDisplay, GameObject reticleDisplay, Sprite displayImage)
    {
        HandheldDisplay displayToReturn = null;
        HandheldReticle reticleToReturn = null;

        GameObject reticleObject = Instantiate(reticleDisplay, ReticleParent, false);
        GameObject displayObject = Instantiate(handheldDisplay, ammoCounterParent, false);

        switch (nextSlot)
        {
            case HandheldSlot.Pickup:
                pickupDisplay = displayObject;
                pickupImage.sprite = displayImage;
                displayToReturn = pickupDisplay.GetComponent<HandheldDisplay>();
                reticleToReturn = reticleObject.GetComponent<HandheldReticle>();
                break;
            case HandheldSlot.WeaponOne:
                pickupDisplay = displayObject;
                pickupImage.sprite = displayImage;
                displayToReturn = pickupDisplay.GetComponent<HandheldDisplay>();
                reticleToReturn = reticleObject.GetComponent<HandheldReticle>();
                break;
            case HandheldSlot.WeaponTwo:
                pickupDisplay = displayObject;
                pickupImage.sprite = displayImage;
                displayToReturn = pickupDisplay.GetComponent<HandheldDisplay>();
                reticleToReturn = reticleObject.GetComponent<HandheldReticle>();
                break;
            case HandheldSlot.Sidearm:
                pickupDisplay = displayObject;
                pickupImage.sprite = displayImage;
                displayToReturn = pickupDisplay.GetComponent<HandheldDisplay>();
                reticleToReturn = reticleObject.GetComponent<HandheldReticle>();
                break;
        }

        handheldObject.OnAttachedDisplay(displayToReturn);
        displayToReturn.OnAttachedDisplay(defaultColor);
        handheldObject.OnAttachedReticle(reticleToReturn);
        reticleToReturn.OnCreateReticle(defaultColor, enemyColor, friendlyColor);
    }

    public virtual void SetCritIndicator(bool isCrit)
    {
        critIndicator.enabled = isCrit;
    }

    public virtual void SetReticleOffset(float offset)
    {
        Debug.Log("reticle offset: " + offset + " canvas height: " + canvas.rect.height);
        HUDOffset = canvas.rect.height * offset;
        OtherHUDParent.anchoredPosition = new Vector2(0f, -HUDOffset);
        MainHUDParent.anchoredPosition = new Vector2(0f,HUDOffset);
    }

    public virtual void SetHUDRectTransformValues(Vector2 posOffset, float rotOffset)
    {

    }

    public void SetActiveHandheld(HandheldSlot handheldSlot)
    {
        nextSlot = handheldSlot;

        // set which slot is active

        switch (nextSlot)
        {
            case HandheldSlot.WeaponOne:
                weaponOneDisplay.SetActive(true);
                weaponTwoDisplay.SetActive(true);
                sidearmDisplay.SetActive(true);
                pickupDisplay.SetActive(true);
                activeHandheld = weaponOneImage;
                if (hasWeaponTwo)
                {

                }
                break;
            case HandheldSlot.WeaponTwo:
                break;
            case HandheldSlot.Sidearm:
                break;
            case HandheldSlot.Pickup:
                break;
        }
    }
}
