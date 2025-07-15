using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 5f;    // how fast to zoom per scroll tick
    [SerializeField] private float minZoom = 2f;    // smallest orthographic size
    [SerializeField] private float maxZoom = 10f;   // largest orthographic size

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Read scroll delta (y = up/down scroll)
        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();

        // Calculate new size, flipping sign so scroll up zooms in
        float newSize = cam.orthographicSize - scrollDelta.y * zoomSpeed * Time.deltaTime;

        // Clamp to allowable range
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }
}
