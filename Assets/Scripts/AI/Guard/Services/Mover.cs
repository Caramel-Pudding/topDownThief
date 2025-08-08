using UnityEngine;

public class Mover : MonoBehaviour
{
    private Vector2 lastVelocity;

    public bool MoveTowards(Vector2 target, float speed, float stopDistance = 0.05f)
    {
        var pos = (Vector2)transform.position;
        var to = target - pos;

        if (to.sqrMagnitude <= stopDistance * stopDistance)
        {
            transform.position = target;
            lastVelocity = Vector2.zero;
            return true;
        }

        var next = Vector2.MoveTowards(pos, target, speed * Time.deltaTime);
        lastVelocity = (next - pos) / Mathf.Max(Time.deltaTime, 1e-6f);
        transform.position = next;

        if (to.sqrMagnitude > 1e-6f)
        {
            var dir = to.normalized;
            transform.right = new Vector3(dir.x, dir.y, 0f);
        }
        return false;
    }

    public void Stop()
    {
        lastVelocity = Vector2.zero;
        // If you use Rigidbody2D, also zero its velocity:
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;
    }
}
