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


    private void Awake()
    {
        
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
        if (WeaponOneMeta != null)
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
        if (WeaponOneMeta != null)
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
        if (m_CurrentHandheldInterface != null)
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
        if (m_CurrentHandheldInterface != null)
            m_CurrentHandheldInterface.OnFire(context);
    }

    /* Might only be necessary for animator
     */
    public void OnJump(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null)
            m_CurrentHandheldInterface.OnJump(context);
    }

    /* Only necessary for animator
     */
    public void OnLook(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null)
            m_CurrentHandheldInterface.OnLook(context);
    }

    /* Only necessary for animator
     */
    public void OnMove(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null)
            m_CurrentHandheldInterface.OnMove(context);
    }

    /* Just pass through to the handheld object
     */
    public void OnReload(InputAction.CallbackContext context)
    {
        if (m_CurrentHandheldInterface != null)
            m_CurrentHandheldInterface.OnReload(context);
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
        if (m_CurrentHandheldInterface != null)
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
    Disable,
    Size
}

public class EquipmentInfo
{
    public GameObject Equipment;
    public int MaxCount;
    public int Count;
}