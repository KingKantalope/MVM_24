using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.UI;

public class HandheldGun : MonoBehaviour, IHandheldObject
{
    private CarrierSystem m_CarrierSystem;
    private Recoil m_Recoil;
    private CapsuleCollider m_MeleeCollider;
    private PlayerHUD m_PlayerHUD;
    private PlayerAim m_PlayerAim;

    #region variables

    [Header("Gun Visuals")]
    [SerializeField] private Animator GunAnimator;
    [SerializeField] private MeshRenderer MeshRender;

    [Header("HUD Elements")]
    [SerializeField] private GameObject ammoDisplayObject;
    [SerializeField] private GameObject reticleDisplayObject;
    [SerializeField] private Sprite displayImage;
    private HandheldDisplay ammoDisplay;
    private HandheldReticle reticleDisplay;
    private GameObject ammoObject;
    private GameObject reticleObject;

    [Header("Fire Mode")]
    [SerializeField] private bool isAutomatic;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackRate;
    [SerializeField] private int burstCount;
    [SerializeField] private float burstRate;
    private float attackTime;
    private int roundNum;

    [Header("Charging")]
    [SerializeField] private bool isChargeable;
    [SerializeField] private float minCharge;
    private float currentCharge;

    [Header("Accuracy and Recoil")]
    [SerializeField] private List<Vector3> recoilPattern;
    [SerializeField] private List<Vector3> recoilVariance;
    [SerializeField] private float maxRecoilReturn;
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    [SerializeField] private float minSpread;
    [SerializeField] private float maxSpread;
    [SerializeField] private float spreadIncreasePerAttack;
    [SerializeField] private float spreadResetRate;
    private float spread;

    [Header("Ammo Consumption")]
    [SerializeField] private bool infiniteAmmo;
    [SerializeField] private int ammoPerAttack;

    [Header("Ammo Capacity")]
    [SerializeField] private int magSize;
    [SerializeField] private int startingMagSize;
    [SerializeField] private int maxReserves;
    [SerializeField] private int startingReserves;
    [SerializeField] private int reservesThreshold;
    [SerializeField] private float lowThreshold;
    protected int magCurrent;
    protected int reservesCurrent;

    [Header("Reloading")]
    [SerializeField] private float mainAddTime;
    [SerializeField] private float mainRemainTime;
    [SerializeField] private float altAddTime;
    [SerializeField] private float altRemainTime;
    private float addTime;
    private float remainTime;
    private float reloadTime;
    private bool hasReloaded;

    [Header("Melee Stuff")]
    [SerializeField] private float meleeHitTime;
    [SerializeField] private float meleeHangTime;
    [SerializeField] private float meleeDamage;
    private float meleeTime;
    private bool hasMeleed;

    [Header("Equipping and Stowing")]
    [SerializeField] private float EquipDuration;
    [SerializeField] private float StowDuration;
    private float switchTime;

    [Header("Aim Assistance")]
    [SerializeField] private float redReticleRange;
    [SerializeField] private float maxAssistRange;
    [SerializeField] private float assistInnerAngle;
    [SerializeField] private float assistOuterAngle;
    [SerializeField] private float assistMaxFriction;
    [SerializeField] private float assistMaxMagnetism;
    [SerializeField] private float autoMaxAngle;

    private bool wantToAttack;
    private bool wantToAltAttack;
    private bool wantToReload;
    private bool wantToMelee;
    private GunAction currentAction = GunAction.Stowed;

    private int testMagCurrent = 8;
    private int testMagSize = 8;
    private float testLowThreshold = 0.4f;

    #endregion variables

    // Start is called before the first frame update
    void Start()
    {
        magCurrent = startingMagSize;
        reservesCurrent = startingReserves;
        currentAction = GunAction.Stowed;
        wantToAttack = false;
        wantToAltAttack = false;
        wantToReload = false;
        wantToMelee = false;

        if (infiniteAmmo)
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold);
        }
        else
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold, reservesCurrent, reservesThreshold);
        }
    }

    void Update()
    {
        // specific updates
        AttackUpdate();
        MeleeUpdate();
        ReloadUpdate();
        EquipUpdate();
        StowUpdate();
        Debug.Log("currentAction: " + currentAction);


        handleInput();
        //RedReticle();
    }

    void LateUpdate()
    {
        handleAccuracy();
    }

    protected virtual void handleInput()
    {
        if (currentAction == GunAction.Idle || currentAction == GunAction.Attacking)
        {
            if (wantToAttack && magCurrent > 0)
            {
                handleTrigger();
            }

            if (wantToReload && magCurrent < magSize && reservesCurrent > 0)
            {
                StartReload();
            }

            if (wantToMelee)
            {
                StartMelee();
            }
        }
    }

    protected virtual void handleTrigger()
    {
        if (isChargeable)
        {
            if (currentCharge < minCharge)
            {
                currentCharge += Time.deltaTime;
            }
            else
            {
                StartAttack();

                currentCharge = 0f;
            }
        }
        else
        {
            StartAttack();
        }
    }

    #region HandheldObject

    public void OnAttachedCarrier(CarrierSystem attachedCarrier)
    {
        m_CarrierSystem = attachedCarrier;
    }

    public void OnAttachedAim(PlayerAim attachedAim)
    {
        m_PlayerAim = attachedAim;

        m_PlayerAim.SetRecoilValues(snappiness, returnSpeed);
    }

    public void OnAttachedMeleeCollider(CapsuleCollider MeleeCollider)
    {
        m_MeleeCollider = MeleeCollider;
    }

    public void OnAttachedHUD(PlayerHUD playerHUD)
    {
        m_PlayerHUD = playerHUD;

        // playerHUD.OnAttachHandheldHUD(this, ammoDisplayObject, reticleDisplayObject, displayImage);

        Transform reticleParent, ammoParent;

        (reticleParent, ammoParent) = m_PlayerHUD.OnAttachHandheldHUDFIXED();

        reticleObject = Instantiate(reticleDisplayObject, reticleParent, false);
        ammoObject = Instantiate(ammoDisplayObject, ammoParent, false);

        reticleDisplay = reticleObject.GetComponent<HandheldReticle>();
        ammoDisplay = ammoObject.GetComponent<HandheldDisplay>();
    }

    public void OnAttachedDisplay(HandheldDisplay handheldDisplay)
    {
        //ammoDisplay = handheldDisplay;
    }

    public void OnAttachedReticle(HandheldReticle handheldReticle)
    {
        reticleDisplay = handheldReticle;
    }

    #region InputActions

    public void OnAltFire(InputAction.CallbackContext context)
    {
        if (context.performed) wantToAltAttack = true;
        else if (context.canceled) wantToAltAttack = false;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed) wantToAttack = true;
        else if (context.canceled) wantToAttack = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {

    }

    public void OnLook(InputAction.CallbackContext context)
    {
        
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed) wantToReload = true;
        else if (context.canceled) wantToReload = false;
    }

    public void OnScrollThroughWeapons(InputAction.CallbackContext context)
    {
        
    }

    public void OnSwapWeapons(InputAction.CallbackContext context)
    {

    }

    public void OnMelee(InputAction.CallbackContext context)
    {
        if (context.performed) wantToMelee = true;
        else if (context.canceled) wantToMelee = false;
    }

    #endregion InputActions

    #endregion HandheldObject

    #region GunAttack

    /* AttackCoroutine accounts for the attack's attackDelay, attackRate,
     * burstCount, burstRate,
     */
    // call this instead of starting the coroutine
    protected virtual void StartAttack()
    {
        if (attackTime <= 0f)
        {
            attackTime = attackDelay;
            currentAction = GunAction.Attacking;
            roundNum = 0;
        }
    }

    // only call this when isAttacking is true
    protected virtual void AttackUpdate()
    {
        // refactor coroutine functionality to work on update
        // this shouldn't be so taxing like all these coroutines are now
        if (attackTime > 0f)
        {
            attackTime -= Time.deltaTime;
        }
        else if (magCurrent > 0 && currentAction == GunAction.Attacking && (wantToAttack || roundNum > 0))
        {
            Attack();
            roundNum++;

            if (roundNum < burstCount)
            {
                attackTime = burstRate;
            }
            else
            {
                roundNum = 0;
                attackTime = attackRate;
                currentAction = GunAction.Idle;
                if (!isAutomatic) wantToAttack = false;
            }
        }
    }

    /* Create a basic projectile spawning system,
     * the scriptable object will deal with multiple projectiles
     */
    protected virtual void Attack()
    {
        // spawn projectile

        // consume ammo
        ConsumeAmmo(ammoPerAttack);

        // play animation
        m_CarrierSystem.GetAnimator().SetTrigger("Attack");
        GunAnimator.SetTrigger("Attack");

        if (infiniteAmmo)
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold);
        }
        else
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold, reservesCurrent, reservesThreshold);
        }

        // increase spread
        spread += spreadIncreasePerAttack;

        // call recoil
        m_PlayerAim.AddRecoil(recoilPattern[roundNum % recoilPattern.Count], recoilVariance[roundNum % recoilVariance.Count], maxRecoilReturn);
    }

    #endregion

    #region GunSpread

    protected virtual void handleAccuracy()
    {
        spread = Mathf.Clamp(spread - (Time.deltaTime * spreadResetRate), minSpread, maxSpread);

        // update HUD
        reticleDisplay.SetReticleSpread(spread);
    }

    #endregion

    #region GunAmmo

    /* */
    protected virtual void ReloadUpdate()
    {
        // refactor coroutine functionality to work on update
        // this shouldn't be so taxing like all these coroutines are now
        if (reloadTime > 0f)
        {
            // wait to add ammo to mag
            reloadTime -= Time.deltaTime;
            Debug.Log("Reloading: " + reloadTime);
        }
        else if (currentAction == GunAction.Reloading)
        {
            if (!hasReloaded)
            {
                // add ammo
                Reload();
                Debug.Log("reloaded");
                hasReloaded = true;
                reloadTime = mainRemainTime;
            }
            else
            {
                // end reload
                currentAction = GunAction.Idle;
                Debug.Log("Going back to Idle");
                // reset reloaded
                hasReloaded = false;

                m_CarrierSystem.GetAnimator().SetBool("isReloading", false);
                GunAnimator.SetBool("isReloading", false);
            }
        }
    }

    protected virtual void StartReload()
    {
        wantToAttack = false;
        wantToAltAttack = false;
        hasReloaded = false;

        bool isEmpty = false;

        if (magCurrent <= 0)
        {
            reloadTime = altAddTime;
            isEmpty = true;
        }
        else
        {
            reloadTime = mainAddTime;
        }

        currentAction = GunAction.Reloading;
        Debug.Log("Reloading?: " + currentAction);

        // play animation
        m_CarrierSystem.GetAnimator().SetBool("isReloading", true);
        GunAnimator.SetBool("isReloading", true);
        m_CarrierSystem.GetAnimator().SetTrigger(isEmpty ? "ReloadNormal" : "ReloadEmpty");
        GunAnimator.SetTrigger(isEmpty ? "ReloadNormal" : "ReloadEmpty");
    }

    protected virtual void Reload()
    {
        if (infiniteAmmo)
        {
            magCurrent = magSize;
        }
        else if ((magCurrent + reservesCurrent) < magSize)
        {
            magCurrent += reservesCurrent;
            reservesCurrent = 0;
        }
        else
        {
            reservesCurrent -= (magSize - magCurrent);
            magCurrent = magSize;
        }

        if (infiniteAmmo)
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold);
        }
        else
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold, reservesCurrent, reservesThreshold);
        }
    }

    protected void ConsumeAmmo(int ammoAmount)
    {
        magCurrent -= ammoAmount;
        if (magCurrent < 0) magCurrent = 0;
        Debug.Log("magCurrent: " + magCurrent);
        
    }

    #endregion

    #region GunSwapping

    public void OnEquip()
    {
        if (currentAction != GunAction.Equipping)
        {
            m_CarrierSystem.GetAnimator().SetTrigger("Equip");
            GunAnimator.SetTrigger("Equip");

            // turn model visible
            MeshRender.enabled = true;

            currentAction = GunAction.Equipping;
            switchTime = EquipDuration;
        }
    }

    public void OnUnequip()
    {
        if (currentAction != GunAction.Stowing)
        {
            m_CarrierSystem.GetAnimator().SetTrigger("Unequip");
            GunAnimator.SetTrigger("Unequip");

            // turn reticle invisible
            reticleDisplay.SetVisibility(false);

            currentAction = GunAction.Stowing;
            switchTime = StowDuration;
        }
    }

    private void EquipUpdate()
    {
        // refactor coroutine functionality to work on update
        // this shouldn't be so taxing like all these coroutines are now

        if (switchTime > 0f)
        {
            switchTime -= Time.deltaTime;
        }
        else if (currentAction == GunAction.Equipping)
        {
            // turn reticle visible
            reticleDisplay.SetVisibility(true);
            currentAction = GunAction.Idle;
        }
    }

    private void StowUpdate()
    {
        // refactor coroutine functionality to work on update
        // this shouldn't be so taxing like all these coroutines are now

        if (switchTime > 0f)
        {
            switchTime -= Time.deltaTime;
        }
        else if (currentAction == GunAction.Stowing)
        {
            // turn model invisible
            MeshRender.enabled = false;

            m_CarrierSystem.FinishHandheldSwap();
            currentAction = GunAction.Stowed;
        }
    }

    #endregion

    #region GunMelee

    protected virtual void StartMelee()
    {
        wantToMelee = false;
        currentAction = GunAction.Meleeing;
        hasMeleed = false;

        //animations
        m_CarrierSystem.GetAnimator().SetTrigger("Melee");
        GunAnimator.SetTrigger("Melee");

        meleeTime = meleeHitTime;
    }

    protected void MeleeUpdate()
    {
        if (meleeTime > 0f)
        {
            meleeTime -= Time.deltaTime;
        }
        else if (!hasMeleed)
        {
            Melee();
            hasMeleed = true;
            meleeTime = meleeHangTime;
        }
        else if (meleeTime > 0f)
        {
            meleeTime -= Time.deltaTime;
        }
        else if (currentAction == GunAction.Meleeing)
        {
            currentAction = GunAction.Idle;
        }
    }

    protected void Melee()
    {
        // call for melee damage/physics
        // use CapsuleCast to find hitboxes with the correct tags
    }

    #endregion

    #region GunAimAssist

    /* Update this method to better 
     */
    private void SetReticleColor()
    {
        bool isEnemy = false;
        bool isFriendly = false;

        // figure out if reticle should be red or not

        // change reticle color based on that information
        reticleDisplay.SetReticleColor(isEnemy, isFriendly);
    }

    #endregion

    #region GunInterruption
    public void OnStartInteruption()
    {
        currentAction = GunAction.Interrupted;
    }

    public void OnStopInteruption()
    {

        currentAction = GunAction.Idle;
    }
    #endregion GunInterruption
}

public enum GunAction
{
    Idle,
    Attacking,
    Reloading,
    Meleeing,
    Equipping,
    Stowing,
    Stowed,
    Interrupted
}