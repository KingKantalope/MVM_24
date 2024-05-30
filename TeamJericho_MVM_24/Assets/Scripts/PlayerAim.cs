using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [Header("Look Offset Stuff")]
    [SerializeField] private Transform armHolder;
    [SerializeField] private Transform camHolder;
    [SerializeField] private Camera cam;
    [Range(-0.0825f, 0.0825f)]
    [SerializeField] private float aimOffset;
    [Range(60f, 120f)]
    [SerializeField] private float defaultFieldOfView;
    [SerializeField] private PlayerHUD playerHUD;
    [Header("Sensitivity and Aim Assist")]
    [SerializeField] private Vector2 mouseSensitivity;
    [SerializeField] private Vector2 gamepadSensitivity;
    private Vector2 mouseLook, gamepadLook;
    private float lookRotation;
    private float gamepadAcceleration;
    private float gamepadFriction, gamepadMagnetism;
    private float mouseFriction, mouseMagnetism;
    private bool stickAim;
    private bool aimLocked = false;

    // zooming
    private float targetZoom = 1;
    private float zoomSpeed;
    private float currentZoomPercent;

    // recoil backend stuff
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Vector3 returnRotation;

    [Header("Recoil Stuff")]
    [SerializeField] private Transform zRotation;
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    public void OnMouseLook(InputAction.CallbackContext context)
    {
        if (!aimLocked) mouseLook = context.ReadValue<Vector2>();
        else mouseLook = Vector2.zero;
    }

    public void OnGamepadLook(InputAction.CallbackContext context)
    {
        if (!aimLocked) gamepadLook = context.ReadValue<Vector2>();
        else gamepadLook = Vector2.zero;
    }

    public void OnPlayerClimb(bool isClimbing)
    {
        aimLocked = isClimbing;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        returnRotation = Vector3.zero;

        currentZoomPercent = 0f;


        SetCameraOffset();
    }

    void Update()
    {
        HandleRecoil();
        playerHUD.SetHUDRectTransformValues(Vector2.zero, 0f); // change these arguments!!!

        // zooming
        if (cam.fieldOfView != defaultFieldOfView * targetZoom)
        {
            UpdateCameraFOV();
        }
    }

    private void LateUpdate()
    {
        GamepadLookAcceleration();
        AimMagnetism();
        Look();
    }

    private void SetTargetZoom(float target, float speed)
    {
        targetZoom = target; // inverse factor of fieldOfView modifier
        zoomSpeed = speed; // degrees per second
    }

    private void ResetTargetZoom(float speed)
    {
        targetZoom = 1f;
        zoomSpeed = speed;
    }

    private void UpdateCameraFOV()
    {
        float changeInFOV = zoomSpeed * Time.deltaTime;
        if (cam.fieldOfView > defaultFieldOfView / targetZoom)
        {
            changeInFOV *= -1f;
        }
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + changeInFOV,defaultFieldOfView / targetZoom,defaultFieldOfView);

        // last thing to do!
        camHolder.localRotation = Quaternion.Euler(-90f - cam.fieldOfView * aimOffset, 0f, 180f);
    }

    private void SetCameraOffset()
    {
        // set camHolder localRotation
        camHolder.localRotation = Quaternion.Euler(-90f + cam.fieldOfView * aimOffset, 0f,180f);
        playerHUD.SetReticleOffset(aimOffset);
    }

    private void UpdateCameraOffset()
    {
    }

    private void HandleRecoil()
    {
        targetRotation = Vector3.Lerp(targetRotation, returnRotation, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        zRotation.localRotation = Quaternion.Euler(new Vector3(0f,0f,currentRotation.z));
    }

    public void SetRecoilValues(float newSnappiness, float newReturnSpeed)
    {
        snappiness = newSnappiness;
        returnSpeed = newReturnSpeed;
    }

    public void AddRecoil(Vector3 recoil, Vector3 variance, float maxMagnitude)
    {
        targetRotation += new Vector3(
            Random.Range(recoil.x - variance.x, recoil.x + variance.x),
            Random.Range(recoil.y - variance.y, recoil.y + variance.y),
            Random.Range(recoil.z - variance.z, recoil.z + variance.z));

        Vector3 netRecoil = new Vector3(targetRotation.x - returnRotation.x, targetRotation.y - returnRotation.y,0f) ;
        
        if (netRecoil.magnitude > maxMagnitude)
        {
            returnRotation = new Vector3(targetRotation.x - (netRecoil.normalized.x * maxMagnitude), targetRotation.y - (netRecoil.normalized.y * maxMagnitude),0f);
        }
    }

    private void Look()
    {
        // prep recoil
        Vector3 netRecoil = targetRotation - currentRotation;
        Vector3 recoilRotation = new Vector3(0f,netRecoil.y,0f);

        // left/right inputs
        Vector3 mouseTurn = Vector3.up * mouseLook.x * mouseSensitivity.x * (1f - mouseFriction);
        Vector3 gamepadTurn = Vector3.up * gamepadLook.x * gamepadSensitivity.x * gamepadAcceleration * (1f - gamepadFriction);

        // turn
        armHolder.Rotate(mouseTurn + gamepadTurn + recoilRotation);

        // up/down inputs
        float mouseRotation = -mouseLook.y * mouseSensitivity.y * (1f - mouseFriction);
        float gamepadRotation = -gamepadLook.y * gamepadSensitivity.y * gamepadAcceleration * (1f - gamepadFriction);

        // look
        lookRotation += (mouseRotation + gamepadRotation + netRecoil.x);
        lookRotation = Mathf.Clamp(lookRotation, -90f, 90f);
        zRotation.eulerAngles = new Vector3(lookRotation, zRotation.eulerAngles.y, zRotation.eulerAngles.z);
    }

    /* Set up acceleration based on the magnitude and duration of gamepad aim input
     * Should not instantly reset when quickly changing from left to right or up to down and vice versa
     */
    private void GamepadLookAcceleration()
    {

    }

    public void AimMagnetism()
    {

    }

    public void UpdateAimAssist(float gpFriction, float gpMagnetism, float mFriction, float mMagnetism)
    {
        gamepadFriction = gpFriction;
        gamepadMagnetism = gpMagnetism;
        mouseFriction = mFriction;
        mouseMagnetism = mMagnetism;
    }
}