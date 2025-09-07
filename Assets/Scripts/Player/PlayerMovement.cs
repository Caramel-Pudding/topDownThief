using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Dash")]
    public float dashDistance = 3f;
    public float dashDuration = 0.1f;

    [Tooltip("Speed multiplier applied after a dash while recovery is active.")]
    public float recoverySpeedMultiplier = 0.1f;

    [Tooltip("Recovery duration after dash ends. During recovery, dash is locked and movement is slowed.")]
    public float recoveryDuration = 2f;

    [Header("Facing Source")]
    [SerializeField] private MonoBehaviour facingSource; // must implement IFacingProvider

    private IFacingProvider facing;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastFacing = Vector2.right;
    private InputSystem_Actions controls;

    private float speedMul = 1f;
    private float slowTimer;

    private bool isDashing;
    private float dashTimer;
    private Vector2 dashVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new InputSystem_Actions();

        facing = facingSource as IFacingProvider ?? GetComponentInParent<IFacingProvider>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
        controls.Player.Jump.performed += OnJump;
    }

    void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;
        controls.Player.Jump.performed -= OnJump;
        controls.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > 0.0001f)
            lastFacing = moveInput.normalized;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isDashing) return;
        if (slowTimer > 0f) return; // lock dash during recovery

        Vector2 dir;
        if (moveInput.sqrMagnitude > 0.0001f)
            dir = moveInput.normalized;
        else if (facing != null && facing.Facing.sqrMagnitude > 0.0001f)
            dir = facing.Facing.normalized;
        else
            dir = lastFacing;

        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        dashVelocity = dir * (dashDistance / dashDuration);
        dashTimer = dashDuration;
        isDashing = true;
    }

    void Update()
    {
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
                speedMul = 1f;
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashVelocity;
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
                speedMul = recoverySpeedMultiplier;
                slowTimer = recoveryDuration; // start recovery lock
            }
        }
        else
        {
            float v = speed * speedMul;
            rb.MovePosition(rb.position + moveInput * v * Time.fixedDeltaTime);
        }
    }
}
