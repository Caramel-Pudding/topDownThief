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

    public System.Action<Vector2> OnDashStart;
    public System.Action<Vector2> OnDashEnd;

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

    // --- Footstep Audio ---
    [Header("Footsteps Audio")]
    [SerializeField] private AudioSource stepSource;
    [SerializeField] private AudioClip[] stepClips;
    [SerializeField, Range(0f, 1f)] private float stepVolume = 0.9f;
    [SerializeField] private float stepDistance = 0.6f;
    [SerializeField] private float minMoveSpeedForSteps = 0.5f;
    [SerializeField] private bool requireGrounded = true;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Vector2 pitchJitter = new Vector2(0.95f, 1.05f);

    private Vector2 lastPos;
    private float traveledSinceLastStep;
    private int lastStepClipIndex = -1;

    // --- Dash Audio ---
    [Header("Dash Audio")]
    [SerializeField] private AudioSource dashSource;
    [SerializeField] private AudioClip dashClip;
    [SerializeField, Range(0f, 1f)] private float dashVolume = 1f;
    [SerializeField] private Vector2 dashPitchJitter = new Vector2(0.95f, 1.05f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new InputSystem_Actions();

        facing = facingSource as IFacingProvider ?? GetComponentInParent<IFacingProvider>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        EnsureStepSource();
        EnsureDashSource();
        lastPos = rb.position;
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
        if (slowTimer > 0f) return;

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
        OnDashStart?.Invoke(dir);

        PlayDashSFX();
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
            FootstepResetWhileDash();

            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
                speedMul = recoverySpeedMultiplier;
                slowTimer = recoveryDuration;
                OnDashEnd?.Invoke(dashVelocity.normalized);
            }
        }
        else
        {
            float v = speed * speedMul;
            Vector2 nextPos = rb.position + moveInput * v * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);
        }

        UpdateFootsteps();
    }

    // --- Footsteps logic (unchanged) ---
    private void UpdateFootsteps()
    {
        if (stepClips == null || stepClips.Length == 0 || stepSource == null) return;
        if (isDashing) { lastPos = rb.position; return; }

        // distance moved this physics tick
        float delta = Vector2.Distance(rb.position, lastPos);
        lastPos = rb.position;

        // derive speed from distance per fixed tick instead of rb.linearVelocity
        float currentSpeed = delta / Time.fixedDeltaTime;

        if (requireGrounded && !IsGrounded())
        {
            traveledSinceLastStep = Mathf.Min(traveledSinceLastStep, stepDistance * 0.5f);
            return;
        }

        bool moving = moveInput.sqrMagnitude > 0.0001f && currentSpeed >= minMoveSpeedForSteps;
        if (!moving) return;

        traveledSinceLastStep += delta;

        if (traveledSinceLastStep >= stepDistance)
        {
            traveledSinceLastStep -= stepDistance;
            PlayStep();
        }
    }


    private void PlayStep()
    {
        int clipIndex = Random.Range(0, stepClips.Length);
        if (stepClips.Length > 1 && clipIndex == lastStepClipIndex)
            clipIndex = (clipIndex + 1) % stepClips.Length;
        lastStepClipIndex = clipIndex;

        var clip = stepClips[clipIndex];
        if (!clip) return;

        float p = Random.Range(pitchJitter.x, pitchJitter.y);
        float prevPitch = stepSource.pitch;
        stepSource.pitch = p;

        stepSource.PlayOneShot(clip, stepVolume);
        stepSource.pitch = prevPitch;
    }

    private bool IsGrounded()
    {
        Vector2 c = groundCheck ? (Vector2)groundCheck.position : rb.position;
        Collider2D[] buf = new Collider2D[1];
        var filter = new ContactFilter2D { useLayerMask = true, useTriggers = false };
        filter.SetLayerMask(groundMask);
        int count = Physics2D.OverlapCircle(c, groundCheckRadius, filter, buf);
        return count > 0;
    }

    private void FootstepResetWhileDash()
    {
        traveledSinceLastStep = 0f;
    }

    private void EnsureStepSource()
    {
        if (!stepSource)
        {
            stepSource = GetComponent<AudioSource>();
            if (!stepSource) stepSource = gameObject.AddComponent<AudioSource>();
        }
        stepSource.playOnAwake = false;
        stepSource.loop = false;
        stepSource.spatialBlend = 0f;
    }

    private void EnsureDashSource()
    {
        if (!dashSource)
        {
            dashSource = gameObject.AddComponent<AudioSource>();
        }
        dashSource.playOnAwake = false;
        dashSource.loop = false;
        dashSource.spatialBlend = 0f;
    }

    private void PlayDashSFX()
    {
        if (!dashClip || dashSource == null) return;

        float p = Random.Range(dashPitchJitter.x, dashPitchJitter.y);
        float prevPitch = dashSource.pitch;
        dashSource.pitch = p;

        dashSource.PlayOneShot(dashClip, dashVolume);
        dashSource.pitch = prevPitch;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (requireGrounded)
        {
            Gizmos.color = Color.cyan;
            Vector3 c = groundCheck ? groundCheck.position : transform.position;
            Gizmos.DrawWireSphere(c, groundCheckRadius);
        }
    }
#endif
}
