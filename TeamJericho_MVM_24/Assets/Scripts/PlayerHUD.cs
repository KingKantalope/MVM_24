using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.NetworkInformation;

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
    [SerializeField] private Color reticleColor;
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

    [SerializeField] private Image healthBar;
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private Color healthColor;
    [SerializeField] private Color healthBackgroundColor;
    [SerializeField] private Color lowColor;
    [SerializeField] private Color lowBackgroundColor;
    [SerializeField] private TMP_Text armorLevelText;
    [SerializeField] private Image armorBar;
    [SerializeField] private Image armorBarBackground;
    [SerializeField] private Color armorColor;
    [SerializeField] private Color armorBackgroundColor;
    [SerializeField] private Image shieldBar;
    [SerializeField] private Image shieldBarBackground;
    [SerializeField] private Color shieldColor;
    [SerializeField] private Color shieldBackgroundColor;

    [SerializeField] private Transform aimParent;
    [SerializeField] private GameObject damageIndicator;
    [SerializeField] private float indicatorDecayRate = 0.2f;
    [SerializeField] private float minIndicatorAlpha = 0.2f;
    private List<DamageIndicator> damageIndicators;

    // Start is called before the first frame update
    void Start()
    {
        damageIndicators = new List<DamageIndicator>();

        // set default rotation
        // set default HUD position
    }

    void LateUpdate()
    {
        // set hud centered on actual aim
        UpdateDamageIndicators();
    }

    public (RectTransform, RectTransform) OnAttachHandheldHUDFIXED()
    {
        return (ReticleParent, ammoCounterParent);
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

    public void SpawnDamageIndicator(Vector3 source, float intensity)
    {
        // check if an indicator already exists for this source
        foreach (var indication in damageIndicators)
        {
            if (indication.source == source)
            {
                indication.indicator.alpha += intensity;
                indication.addedThisFrame = true;
                return;
            }
        }

        Debug.Log("Damage Indicator Created");

        // instantiate indicator and pass it its source and intensity
        GameObject indicatorObject = Instantiate(damageIndicator, ReticleParent);
        DamageIndicator indicator = new DamageIndicator();
        indicator.indicator = indicatorObject.GetComponent<CanvasGroup>();
        indicator.source = source;
        indicator.addedThisFrame = true;
        indicator.indicator.alpha = intensity + minIndicatorAlpha;

        // add to list of indicators
        damageIndicators.Add(indicator);
    }

    public void UpdateDamageIndicators()
    {
        Vector3 toSource = Vector3.zero;
        float dirRight = 0f;
        float dirUp = 0f;
        float indicatorRot = 0f;

        foreach (var indication in damageIndicators)
        {
            if (indication.indicator.alpha <= 0f)
            {
                // remove from list
                damageIndicators.Remove(indication);

                // destroy the indicator
                Destroy(indication.indicator.gameObject);
            }
            else
            {
                // fade indicator
                if (!indication.addedThisFrame) indication.indicator.alpha -= Time.deltaTime * indicatorDecayRate;
                else indication.addedThisFrame = false;

                // get angle from camera to source
                toSource = indication.source - aimParent.position;
                dirRight = Vector3.Dot(aimParent.right, toSource.normalized);
                dirUp = Vector3.Dot(aimParent.up, toSource.normalized);
                indicatorRot = -Mathf.Atan2(dirUp,dirRight) * Mathf.Rad2Deg;

                // rotate indicator
                indication.indicator.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0f,0f,indicatorRot - 90f));
            }
        }
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

    public void SetHealth(float healthCurrent, float healthMax)
    {
        float healthRatio = healthCurrent / healthMax;
        healthBar.fillAmount = healthRatio;

        if (healthRatio < 0.35)
        {
            healthBar.color = lowColor;
            healthBarBackground.color = lowBackgroundColor;
        }
        else
        {
            healthBar.color = healthColor;
            healthBarBackground.color = healthBackgroundColor;
        }
    }

    public void SetArmor(float armorCurrent, float armorMax, int armorLevel, int armorWeakening)
    {
        if (armorWeakening > 0) armorLevelText.color = Color.green;
        else armorLevelText.color = Color.black;

        armorLevelText.text = (armorLevel - armorWeakening).ToString();

        float armorRatio = armorCurrent / armorMax;
        armorBar.fillAmount = armorRatio;

        if (armorRatio < 0.35)
        {
            armorBar.color = lowColor;
            armorBarBackground.color = lowBackgroundColor;
        }
        else
        {
            armorBar.color = armorColor;
            armorBarBackground.color = armorBackgroundColor;
        }
    }

    public void SetShields(float shieldsCurrent, float shieldsMax)
    {
        float shieldRatio = shieldsCurrent / shieldsMax;
        shieldBar.fillAmount = shieldRatio;

        if (shieldRatio < 0.35)
        {
            shieldBar.color = lowColor;
            shieldBarBackground.color = lowBackgroundColor;
        }
        else
        {
            shieldBar.color = shieldColor;
            shieldBarBackground.color = shieldBackgroundColor;
        }
    }
}

public class DamageIndicator
{
    public CanvasGroup indicator;
    public Vector3 source;
    public bool addedThisFrame;
}