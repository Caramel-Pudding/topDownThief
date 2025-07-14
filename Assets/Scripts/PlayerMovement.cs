using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Units per second")]
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private InputSystem_Actions controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new InputSystem_Actions();
    }

    void OnEnable()
    {
        // Enable the Player action map
        controls.Player.Enable();

        // Subscribe to Move started/changed/canceled
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
    }

    void OnDisable()
    {
        // Unsubscribe and disable the map
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;
        controls.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        // Read the 2D vector; canceled will send (0,0)
        moveInput = ctx.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Move via Rigidbody2D for proper collision
        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }
}