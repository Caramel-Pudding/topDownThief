using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class RotateWithPhysics : MonoBehaviour
{
    [SerializeField] private float offsetAngle = 0f;  // adjust this in the Inspector if sprite isn't facing right by default
    private Rigidbody2D rb;
    private Camera mainCam;
    private float camZDistance;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        // Calculate the distance on the Z axis between the camera and the sprite plane
        camZDistance = Mathf.Abs(mainCam.transform.position.z);
    }

    void FixedUpdate()
    {
        // 1) Read mouse position in screen space
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        // 2) Convert screen coordinates to world coordinates (include Z distance)
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, camZDistance)
        );

        // 3) Compute direction vector from the Rigidbody2D to the cursor
        Vector2 direction = mouseWorld - (Vector3)rb.position;

        // 4) Calculate angle in degrees and apply offset
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 5) Rotate the Rigidbody2D using physics
        rb.MoveRotation(angle + offsetAngle);
    }
}
