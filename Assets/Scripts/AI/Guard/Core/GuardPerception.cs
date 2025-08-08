using UnityEngine;

public class GuardPerception : MonoBehaviour
{
    public Transform eyes;
    public GuardConfig config;

    public bool TryGetVisibility(Transform target, out float factor)
    {
        factor = 0f;
        if (!target || !config) return false;

        var origin = eyes ? eyes.position : transform.position;
        var to = (Vector2)(target.position - origin);
        var dist = to.magnitude;
        if (dist > config.visionRadius) return false;

        float angle = Vector2.Angle(transform.right, to.normalized);
        if (angle > config.fov * 0.5f) return false;

        var hit = Physics2D.Raycast(origin, to.normalized, dist, config.obstaclesMask);
        if (hit.collider != null) return false;

        float angleFactor = 1f - Mathf.Clamp01(angle / (config.fov * 0.5f));
        float distFactor = 1f - Mathf.Clamp01(dist / config.visionRadius);
        factor = Mathf.Clamp01(angleFactor * 0.6f + distFactor * 0.4f);
        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (!config) return;
        var origin = eyes ? eyes.position : transform.position;
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(origin, config.visionRadius);

        Vector2 fwd = transform.right;
        float half = config.fov * 0.5f * Mathf.Deg2Rad;
        Vector2 a = Rotate(fwd, +half);
        Vector2 b = Rotate(fwd, -half);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + (Vector3)a * config.visionRadius);
        Gizmos.DrawLine(origin, origin + (Vector3)b * config.visionRadius);
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        float ca = Mathf.Cos(radians);
        float sa = Mathf.Sin(radians);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }
}
