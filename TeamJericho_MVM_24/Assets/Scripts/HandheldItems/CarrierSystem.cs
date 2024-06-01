using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarrierSystem : MonoBehaviour, Controls.IPlayerActions
{
    [Header("Sockets")]
    [SerializeField] private Transform RigSocket;

    [Header("Other Important Scripts")]
    [SerializeField] private Animator RigAnimator;
    [SerializeField] private PlayerMove Movement;
    [SerializeField] private CapsuleCollider MeleeCollider;
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private PlayerHitpoints playerHitpoints;

    [Header("Weapons")]
    [SerializeField] private HandheldSlot startingSlot = HandheldSlot.None;
    [SerializeField] private HandheldScriptableObject WeaponOneMeta;
    [SerializeField] private HandheldScriptableObject WeaponTwoMeta;
    [SerializeField] private HandheldScriptableObject SidearmMeta;

    private HandheldScriptableObject m_CurrentHandheldScriptableObject;
    private GameObject m_CurrentHandheldGameObject;
    private IHandheldObject m_CurrentHandheldInterface;

    private IHandheldObject WeaponOne;
    private IHandheldObject WeaponTwo;
    private IHandheldObject Sidearm;
    private GameObject WeaponOneObject;
    private GameObject WeaponTwoObject;
    private GameObject SidearmObject;
    private HandheldSlot currentSlot = HandheldSlot.None;
    private HandheldSlot nextSlot = HandheldSlot.None;
    private InventoryAction currentAction = InventoryAction.Weapon;

    // swap input variables
    [SerializeField] private float swapThreshold = 0.6f;
    private bool wantToSwap = false;
    private float swapTime = 0.0f;

    [Header("Grenades and Equipment")]
    // create scripts for grenades
    [SerializeField] private EquipmentInfo currentEquipment;

    [Header("Healing Syringes")]
    [SerializeField] private int SyringeMaxCount;
    [SerializeField] private int SyringeCount;
    [SerializeField] private float syringeDelay = 0.4f;
    [SerializeField] private float syringeDuration = 0.8f;
    [SerializeField] private float syringeEnd = 0.3f;
    [SerializeField] private float syringeHealRate = 50f;
    private float syringeTime = 0f;
    private bool isInjecting = false;
    private bool hasInjected = false;


    private void Start()
    {
        InitializeLoadout();
    }

    private void Update()
    {
        RigAnimator.SetFloat("MoveSpeed",Movement.GetMovementRatio());

        // swap fucntionality
        if (wantToSwap)
        {
            if (swapTime >= swapThreshold)
            {
                StartHandheldSwap(true);
                wantToSwap = false;
            }
            else
            {
                swapTime += Time.deltaTime;
            }
        }
    }

    public void SyringeUpdate()
    {
        // syringeDelay -> syringeDuration -> syringeEnd
        if (hasInjected)
        {
            // cooldown after injection
            if (syringeTime > 0f) syringeTime -= Time.deltaTime;
            else
            {
                hasInjected = false;
                isInjecting = false;
                currentAction = InventoryAction.Weapon;
            }
        }
        else if (isInjecting)
        {
            // duration of injection
            if (syringeTime > 0f)
            {
                syringeTime -= Time.deltaTime;

                // tell HitpointHandler to heal based on rate
                playerHitpoints.Heal(syringeHealRate * Time.deltaTime);
            }
            else
            {
                hasInjected = true;
                syringeTime = syringeDuration;
            }
        }
        else
        {
            // delay before injection
            if (syringeTime > 0f) syringeTime -= Time.deltaTime;
            else
            {
                isInjecting = true;
                syringeTime = syringeDuration;
            }
        }
    }

    public Animator GetAnimator()
    {
        return RigAnimator;
    }

    public void InitializeLoadout()
    {
        // instantiate all weapons
        // set up initial weapon
        if (WeaponOneMeta != null)
        {
            WeaponOneObject = Instantiate(WeaponOneMeta.HandheldPrefab, RigSocket, true);
            WeaponOneObject.transform.localPosition = Vector3.zero;
            WeaponOneObject.transform.localRotation = Quaternion.identity;

            WeaponOne = WeaponOneObject.GetComponent<IHandheldObject>();

            // give Sidearm references
            WeaponOne.OnAttachedCarrier(this);
            WeaponOne.OnAttachedAim(playerAim);
            WeaponOne.OnAttachedHUD(playerHUD);

            // assign current weapon
            currentSlot = HandheldSlot.WeaponOne;
            m_CurrentHandheldScriptableObject = WeaponOneMeta;
            m_CurrentHandheldGameObject = WeaponOneObject;
            m_CurrentHandheldInterface = WeaponOne;
        }
        else Debug.Log("WeaponOne is null!");

        if (WeaponTwoMeta != null)
        {
            WeaponTwoObject = Instantiate(WeaponTwoMeta.HandheldPrefab, RigSocket, true);
            WeaponTwoObject.transform.localPosition = Vector3.zero;
            WeaponTwoObject.transform.localRotation = Quaternion.identity;

            WeaponTwo = WeaponTwoObject.GetComponent<IHandheldObject>();

            // give Sidearm references
            WeaponTwo.OnAttachedCarrier(this);
            WeaponTwo.OnAttachedAim(playerAim);
            WeaponTwo.OnAttachedHUD(playerHUD);

            // assign current weapon
            if (currentSlot == HandheldSlot.None)
            {
                currentSlot = HandheldSlot.WeaponTwo;
                m_CurrentHandheldScriptableObject = WeaponTwoMeta;
                m_CurrentHandheldGameObject = WeaponTwoObject;
                m_CurrentHandheldInterface = WeaponTwo;
            }
        }
        else Debug.Log("WeaponTwo is null!");

        if (SidearmMeta != null)
        {
            SidearmObject = Instantiate(SidearmMeta.HandheldPrefab, RigSocket, true);
            SidearmObject.transform.localPosition = Vector3.zero;
            SidearmObject.transform.localRotation = Quaternion.identity;

            Sidearm = SidearmObject.GetComponent<IHandheldObject>();

            // give Sidearm references
            Sidearm.OnAttachedCarrier(this);
            Sidearm.OnAttachedAim(playerAim);
            Sidearm.OnAttachedHUD(playerHUD);

            // assign current weapon
            if (currentSlot == HandheldSlot.None)
            {
                currentSlot = HandheldSlot.Sidearm;
                m_CurrentHandheldScriptableObject = SidearmMeta;
                m_CurrentHandheldGameObject = SidearmObject;
                m_CurrentHandheldInterface = Sidearm;
            }
        }
        else Debug.Log("Sidearm is null!");

        if (currentSlot != HandheldSlot.None)
        {
            // equip weapon
            RigAnimator.runtimeAnimatorController = m_CurrentHandheldScriptableObject.ArmController;
            m_CurrentHandheldInterface.OnEquip();
        }
    }

    public void StartHandheldSwap(bool holdSwap)
    {
        // tell weapon to perform unequip
        m_CurrentHandheldInterface.OnUnequip();

        // get which slot to swap to
        switch (currentSlot)
        {
            case HandheldSlot.WeaponOne:
                if (holdSwap && SidearmMeta != null) nextSlot = HandheldSlot.Sidearm;
                else if (WeaponTwoMeta != null) nextSlot = HandheldSlot.WeaponTwo;
                break;
            case HandheldSlot.WeaponTwo:
                if (holdSwap && SidearmMeta != null) nextSlot = HandheldSlot.Sidearm;
                else if (WeaponOneMeta != null) nextSlot = HandheldSlot.WeaponOne;
                break;
            case HandheldSlot.Sidearm:
                if (WeaponOneMeta != null) nextSlot = HandheldSlot.WeaponOne;
                else if (WeaponOneMeta != null) nextSlot = HandheldSlot.WeaponTwo;
                break;
            case HandheldSlot.None:
                // ???????????
                break;
        }
    }

    public void FinishHandheldSwap()
    {
        // make next weapon the current weapon
        currentSlot = nextSlot;
        playerHUD.SetActiveHandheld(currentSlot);

        // set current handheld references
        switch (currentSlot)
        {
            case HandheldSlot.WeaponOne:
                m_CurrentHandheldScriptableObject = WeaponOneMeta;
                m_CurrentHandheldGameObject = WeaponOneObject;
                m_CurrentHandheldInterface = WeaponOne;
                break;
            case HandheldSlot.WeaponTwo:
                m_CurrentHandheldScriptableObject = WeaponTwoMeta;
                m_CurrentHandheldGameObject = WeaponTwoObject;
                m_CurrentHandheldInterface = WeaponTwo;
                break;
            case HandheldSlot.Sidearm:
                m_CurrentHandheldScriptableObject = SidearmMeta;
                m_CurrentHandheldGameObject = SidearmObject;
                m_CurrentHandheldInterface = Sidearm;
                break;
            case HandheldSlot.Pickup:
                break;
            default:
                break;
        }

        // tell it to equip
        m_CurrentHandheldInterface.OnEquip();
    }

    #region InputActions

    /* Just pass through to the handheld object
     */
    public void OnAltFire(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null && currentAction == InventoryAction.Weapon)
            m_CurrentHandheldInterface.OnAltFire(context);
    }

    /* Might only be necessary for animator
     */
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null)
            m_CurrentHandheldInterface.OnCrouch(context);
    }

    /* Just pass through to the handheld object
     */
    public void OnFire(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null && currentAction == InventoryAction.Weapon)
            m_CurrentHandheldInterface.OnFire(context);
    }

    /* Might only be necessary for animator
     */
    public void OnJump(InputAction.CallbackContext context)
    {
        // animator stuff
    }

    /* Only necessary for animator
     */
    public void OnLook(InputAction.CallbackContext context)
    {
        // animator stuff
    }

    /* Only necessary for animator
     */
    public void OnMove(InputAction.CallbackContext context)
    {
        // animator stuff
    }

    /* Just pass through to the handheld object
     */
    public void OnReload(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null && currentAction == InventoryAction.Weapon)
            m_CurrentHandheldInterface.OnReload(context);
    }

    public void OnUseSyringe(InputAction.CallbackContext context)
    {
        // check if there are any syringes to use
        if (SyringeCount > 0 && currentAction == InventoryAction.Weapon)
        {
            currentAction = InventoryAction.Syringe;
            syringeTime = syringeDelay;
        }
    }

    /* This function needs to account for the two main weapons + sidearm system
     * that needs to be implemented. This means going into the unity editor to
     * include a hold functionality so that the code can be simplified significantly
     */
    public void OnSwapWeapons(InputAction.CallbackContext context)
    {
        // figure this out?
        wantToSwap = context.ReadValueAsButton();

        if (!wantToSwap && swapTime < swapThreshold)
        {
            StartHandheldSwap(false);
        }
    }

    public void OnScrollThroughWeapons(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null)
            m_CurrentHandheldInterface.OnScrollThroughWeapons(context);
    }

    public void OnMelee(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null && currentAction == InventoryAction.Weapon)
            m_CurrentHandheldInterface.OnMelee(context);
    }

    #endregion
}

public enum HandheldSlot
{
    WeaponOne,
    WeaponTwo,
    Sidearm,
    Pickup,
    None,
    Size
}

public enum InventoryAction
{
    Weapon,
    Grenade,
    Syringe,
    Melee,
    Disable,
    Size
}

public class EquipmentInfo
{
    public GameObject Equipment;
    public int MaxCount;
    public int Count;
}