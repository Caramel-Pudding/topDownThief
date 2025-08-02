using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // player
    public Vector3 offset = new Vector3(0, 0, -10); // -10 to see z-9+ objects

    void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + offset;
    }
}