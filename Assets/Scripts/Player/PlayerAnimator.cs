using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private InputSystem_Actions controls;
    private Animator animator;
    private InputAction moveAction;
    private InputAction runAction;

    void Awake()
    {
        controls = new InputSystem_Actions();
        animator = GetComponent<Animator>();
        moveAction = controls.FindAction("Move");   // Vector2
        runAction = controls.FindAction("Sprint");    // Button
    }

    void OnEnable()
    {
        moveAction.Enable();
        runAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        runAction.Disable();
    }

    void Update()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        bool isRun = runAction.ReadValue<float>() > 0.5f;

        float speed = move.magnitude;

        animator.SetFloat("moveX", move.x);
        animator.SetFloat("moveY", move.y);
        animator.SetBool("isRunning", isRun);
        animator.SetFloat("speed", speed);
    }
}