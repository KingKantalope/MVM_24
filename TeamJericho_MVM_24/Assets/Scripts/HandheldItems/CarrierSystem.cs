using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarrierSystem : MonoBehaviour, Controls.IPlayerActions
{
    // private HUD m_HUD;

    [Header("Sockets")]
    [SerializeField] private Transform RigSocket;
    [SerializeField] private Transform SlingSocketOne;
    [SerializeField] private Transform SlingSocketTwo;
    [SerializeField] private Transform SidearmSocket;
    [SerializeField] private Transform EquipmentSocket;

    [Header("Other Important Scripts")]
    [SerializeField] private Animator RigAnimator;
    [SerializeField] private Recoil CameraController;
    [SerializeField] private PlayerMove Movement;
    [SerializeField] private CapsuleCollider MeleeCollider;
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private PlayerAim playerAim;

    [Header("Arsenal")]
    [SerializeField] private IHandheldObject WeaponOne;
    [SerializeField] private IHandheldObject WeaponTwo;
    [SerializeField] private IHandheldObject Sidearm;
    // create throwable script for grenades and 
    [SerializeField] private int FragMaxCount;
    [SerializeField] private int MoverMaxCount;
    [SerializeField] private int PhosphorusMaxCount;
    [SerializeField] private int BismuthMaxCount;
    [SerializeField] public List<HandheldScriptableObject> EquipableHandhelds;

    private HandheldScriptableObject m_CurrentHandheldScriptableObject;
    private GameObject m_CurrentHandheldGameObject;
    private IHandheldObject m_CurrentHandheldInterface;
    private int m_CurrentHandheldIndex;
    private int m_NextHandheldIndex;

    private void Awake()
    {
        SwitchHandheld(EquipableHandhelds[0]);
    }

    private void Update()
    {
        RigAnimator.SetFloat("MoveSpeed",Movement.GetMovementRatio());
        // Debug.Log(Movement.GetMovementRatio());
    }

    public Animator GetAnimator()
    {
        return RigAnimator;
    }

    // needs to change, should use persistent Handhelds to maintain data and timers
    public void SwitchHandheld(HandheldScriptableObject handheld)
    {
        if (m_CurrentHandheldScriptableObject == handheld)
            return;

        Destroy(m_CurrentHandheldGameObject);

        m_CurrentHandheldScriptableObject = handheld;
        m_CurrentHandheldGameObject = Instantiate(m_CurrentHandheldScriptableObject.HandheldPrefab, RigSocket, true);
        m_CurrentHandheldGameObject.transform.localPosition = Vector3.zero;
        m_CurrentHandheldGameObject.transform.localRotation = Quaternion.identity;

        m_CurrentHandheldInterface = m_CurrentHandheldGameObject.GetComponentInChildren<IHandheldObject>();
        if (m_CurrentHandheldInterface != null)
        {
            playerHUD.SetActiveHandheld(HandheldSlot.Sidearm);
            m_CurrentHandheldInterface.OnAttachedCarrier(this);
            m_CurrentHandheldInterface.OnAttachedAim(playerAim);
            m_CurrentHandheldInterface.OnAttachedHUD(playerHUD);
            RigAnimator.runtimeAnimatorController = handheld.ArmController;
            m_CurrentHandheldInterface.OnEquip();
        }
        else
        {
            DestroyImmediate(m_CurrentHandheldGameObject);

            m_CurrentHandheldScriptableObject = null;
            m_CurrentHandheldInterface = null;
            m_CurrentHandheldGameObject = null;
        }
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
        if (context.performed)
        {
            m_NextHandheldIndex = m_CurrentHandheldIndex;
            m_NextHandheldIndex += 1 *(int)Mathf.Sign(context.ReadValue<float>());
            m_NextHandheldIndex %= EquipableHandhelds.Count;

            m_CurrentHandheldInterface.OnUnequip();
        }

        if (m_CurrentHandheldInterface != null)
            m_CurrentHandheldInterface.OnSwapWeapons(context);
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

    public void OnStow()
    {
        SwitchHandheld(EquipableHandhelds[m_NextHandheldIndex]);
    }
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