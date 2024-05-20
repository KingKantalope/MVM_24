using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMove : MonoBehaviour
{
    // events
    public UnityEvent<bool> OnPlayerClimb;
    public UnityEvent<float> OnPlayerMove;
    public UnityEvent OnPlayerJump;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Vector3 playerVelocity;
    private bool grounded;
    private bool canClimb;
    private bool crouching;
    private float slopeAngle;
    private Vector3 slopeNormal;
    private Vector3 wallNormal;
    private float wallAngle;
    private float distToGround;

    public float playerSpeed = 5.0f;
    public float playerAccel = 20.0f;
    public float crouchSpeed = 3.25f;
    public float crouchAccel = 13.0f;
    public float minVelocity = 0.001f;
    public float airAccel = 3.0f;
    public float jumpHeight = 2.0f;
    public float jumpCrouchMulti = 0.5f;
    public float defaultGravity = -10.0f;
    public float maxSlopeAngle = 45.0f;
    public LayerMask whatIsGround = LayerMask.GetMask("Ground");

    // climbing
    public float climbSpeed = 6.0f;
    public float minClimbAngle = 75.0f;
    public float maxClimbAngle = 105.0f;
    public LayerMask whatIsClimbable = LayerMask.GetMask("Climbable");

    private Vector2 moveInput;
    private bool wantToJump;
    private bool wantToCrouch;
    private Vector3 TargetVelocity;
    private Vector3 LateralVelocity;
    private Vector3 newVelocity;

    public List<gravityPair> GravityList;

    public void OnMove(InputAction.CallbackContext context) { moveInput = context.ReadValue<Vector2>(); }
    public void OnJump(InputAction.CallbackContext context)
    {
        wantToJump = context.ReadValueAsButton();

        if (grounded && wantToJump) Jump();
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        wantToCrouch = context.ReadValueAsButton();

        crouching = context.ReadValueAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        rb.sleepThreshold = minVelocity;
        rb.useGravity = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FindClimbWall();
        if (wantToJump && canClimb)
        {
            Climb();
        }
        else
        {
            FindGround();
            LateralMove();
            VerticalMove();
        }
        // Debug.Log("rb.velocity.magnitude: " + rb.velocity.magnitude);
    }

    private void FindClimbWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, playerCollider.radius + 0.5f, whatIsClimbable))
        {
            // save normal of surface
            wallNormal = hit.normal;
            Debug.Log("wall being hit: " + hit.collider.name);

            // get slope angle
            wallAngle = Vector3.Angle(Vector3.up, hit.normal);

            // check slope angle
            if (wallAngle >= minClimbAngle && wallAngle <= maxClimbAngle)
            {
                canClimb = true;
                return;
            }
        }

        canClimb = false;

        // Debug.Log("Cannot Climb!!!!!!!!!!!!");
    }

    private void FindGround()
    {
        // raycast to find ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, (playerCollider.height / 2) + 0.2f, whatIsGround))
        {
            // save normal of surface
            slopeNormal = hit.normal;

            // get slope angle
            slopeAngle = Vector3.Angle(Vector3.up, hit.normal);

            // check slope angle
            if (slopeAngle <= maxSlopeAngle)
            {
                grounded = true;
                return;
            }
        }

        // update this to get all contacts with rigidbody of 45° angle or less steepness
        // average them all out to get proper move vector without relying on being perfectly center
        // should require player to be literally touching ground to be grounded
        
        grounded = false;

        // Debug.Log("Not Grounded!!!!!!!!!!!!");
    }

    private void LateralMove()
    {
        Vector3 velocityChange = Vector3.zero;

        if (grounded)
        {
            // get input relative to slope normal and player rotation
            TargetVelocity = Vector3.zero;
            TargetVelocity += Vector3.ProjectOnPlane(transform.right, slopeNormal).normalized * moveInput.x;
            TargetVelocity += Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized * moveInput.y;

            // project lateralVelocity onto normal
            LateralVelocity = rb.velocity;
            LateralVelocity = Vector3.ProjectOnPlane(LateralVelocity, slopeNormal);

            // get target velocity
            TargetVelocity *= (crouching ? crouchSpeed : playerSpeed);

            velocityChange = (TargetVelocity - LateralVelocity).normalized;
            velocityChange *= (crouching ? crouchAccel : playerAccel) * Time.fixedDeltaTime;

            // change move velocity
            if (rb.velocity.magnitude >= minVelocity || moveInput != Vector2.zero) rb.AddForce(velocityChange, ForceMode.VelocityChange);
            else rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
        }
        else
        {
            // get input relative to player transform rotation
            TargetVelocity = new Vector3(moveInput.x, 0f, moveInput.y);
            TargetVelocity = transform.TransformDirection(TargetVelocity);

            // get lateral velocity
            LateralVelocity = rb.velocity;
            LateralVelocity.y = 0f;

            // get target velocity
            TargetVelocity *= airAccel * Time.fixedDeltaTime;
            TargetVelocity += LateralVelocity;

            // do not accelerate beyond either max speed or current airspeed
            float magnitude = Mathf.Max((crouching ? crouchSpeed : playerSpeed),LateralVelocity.magnitude);
            TargetVelocity = Vector3.ClampMagnitude(TargetVelocity, magnitude);

            velocityChange = TargetVelocity - LateralVelocity;

            // change move velocity
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    private void VerticalMove()
    {
        if (!grounded)
        {
            // gravity!
            rb.AddForce(Vector3.up * defaultGravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    private void Jump()
    {
        Vector3 jumpForces = rb.velocity;

        float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y * (crouching ? jumpCrouchMulti : 1f));

        if (grounded)
        {
            jumpForces.y = jumpVelocity;
        }

        rb.velocity = jumpForces;
    }

    private void Climb()
    {
        Vector3 climbDirection = Vector3.ProjectOnPlane(Vector3.up, wallNormal).normalized;

        rb.velocity = climbDirection * climbSpeed - wallNormal;
    }

    public void AddGravitySource(gravityPair gravitySource)
    {
        // worry about this later
    }

    public float GetMovementRatio()
    {
        return (new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude / playerSpeed);
    }
}

public struct gravityPair
{
    public string source;
    public Vector3 gravity;
}
