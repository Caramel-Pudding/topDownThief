using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class RotateAndAnimate : MonoBehaviour
{
    [SerializeField] private float offsetAngle = 0f;

    private Rigidbody2D rb;
    private Camera mainCam;
    private float camZDistance;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mainCam = Camera.main;
        camZDistance = Mathf.Abs(mainCam.transform.position.z);
    }

    void FixedUpdate()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, camZDistance)
        );

        Vector2 direction = mouseWorld - (Vector3)rb.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle + offsetAngle);

        Vector2 normDir = direction.normalized;
        animator.SetFloat("DirX", normDir.x);
        animator.SetFloat("DirY", normDir.y);
    }
}
