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
    private bool grounded = false;
    private bool canClimb;
    private bool crouching;
    private float slopeAngle;
    private Vector3 slopeNormal;
    private Vector3 wallNormal;
    private float wallAngle;
    private float distToGround;
    private List<Collision> collisions;
    private Vector3 NetNormal = Vector3.zero;

    public float playerSpeed = 5.5f;
    public float playerAccel = 40.0f;
    public float crouchSpeed = 3.25f;
    public float crouchAccel = 13.0f;
    public float minVelocity = 0.4f;
    public float airAccel = 3.0f;
    public float jumpHeight = 2.0f;
    public float jumpCrouchMulti = 0.5f;
    public float defaultGravity = -10.0f;
    public float maxSlopeAngle = 45.0f;
    public float normalForce = 0.1f;
    public LayerMask whatIsGround = LayerMask.GetMask("Ground");

    // climbing
    public float climbSpeed = 6.0f;
    public float minClimbAngle = 75.0f;
    public float maxClimbAngle = 105.0f;
    public LayerMask whatIsClimbable = LayerMask.GetMask("Climbable");
    private bool isClimbing = false;

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
        collisions = new List<Collision>();
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
            if (!isClimbing)
            {
                isClimbing = true;
                OnPlayerClimb?.Invoke(true);
            }

            Climb();
        }
        else
        {
            if (isClimbing)
            {
                isClimbing = false;
                OnPlayerClimb?.Invoke(false);
            }

            NewFindGround();
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

    private void OnCollisionStay(Collision collision)
    {
        collisions.Add(collision);
    }

    private void NewFindGround()
    {
        // get all contact points with collider, find all with hits with a slope below minimum, average them
        Vector3 NetNormal = Vector3.zero;
        RaycastHit hit;
        grounded = false;

        if (collisions.Count > 0)
        {
            foreach (var collision in collisions)
            {
                foreach (var contact in collision.contacts)
                {
                    if (Physics.Raycast(transform.position, contact.point - transform.position, out hit,
                        1.1f * Vector3.Distance(contact.point, transform.position), whatIsGround))
                    {
                        if (Vector3.Angle(Vector3.up, hit.normal) < maxSlopeAngle)
                        {
                            NetNormal += hit.normal;
                        }
                    }
                }
            }
        }

        // raycast to find ground
        if (Physics.Raycast(transform.position, -transform.up, out hit, (playerCollider.height / 2) + 0.2f, whatIsGround))
        {
            // save normal of surface
            NetNormal += hit.normal;
        }

        if (NetNormal != Vector3.zero)
        {
            grounded = true;
            slopeNormal = NetNormal.normalized;
            slopeAngle = Vector3.Angle(Vector3.up, slopeNormal);
        }

        collisions.Clear();
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

            if (velocityChange.y > 0f) velocityChange.y = 0f;

            velocityChange -= slopeNormal.normalized * normalForce;

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
