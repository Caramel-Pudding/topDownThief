using UnityEngine;
using UnityEngine.InputSystem;

public class AimLightOnly : MonoBehaviour
{
    [SerializeField] private float offsetAngle = 0f;
    [SerializeField] private Transform lightHolder;
    [SerializeField] private SpriteRenderer spriteHolder;

    private Camera mainCam;
    private float camZDistance;
    private Animator animator;

    void Awake()
    {
        mainCam = Camera.main;
        camZDistance = Mathf.Abs(mainCam.transform.position.z);
        animator = spriteHolder.GetComponent<Animator>();
    }

    void Update()
    {
        // Get mouse position in world space
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, camZDistance)
        );

        // Calculate direction and angle
        Vector2 direction = (mouseWorld - transform.position).normalized;


        // If the direction is zero, set it to the right
        spriteHolder.flipX = direction.x < 0f;

        animator.SetFloat("dirX", direction.x);
        animator.SetFloat("dirY", direction.y);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotate only the light holder
        if (lightHolder != null)
            lightHolder.rotation = Quaternion.Euler(0f, 0f, angle - 90f + offsetAngle);

        // Pass direction to animator
        animator.SetFloat("dirX", direction.x);
        animator.SetFloat("dirY", direction.y);
    }
}
