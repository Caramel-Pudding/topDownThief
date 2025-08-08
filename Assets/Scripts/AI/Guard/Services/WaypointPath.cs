using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool useChildren = true;

    public int Count => points?.Length ?? 0;
    public bool Loop => loop;
    public Transform GetPoint(int i) => points[i];

    void OnValidate()
    {
        if (useChildren)
        {
            int n = transform.childCount;
            points = new Transform[n];
            for (int i = 0; i < n; i++) points[i] = transform.GetChild(i);
        }
    }

    void OnDrawGizmos()
    {
        if (points == null || points.Length == 0) return;
        Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 1f);
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;
            Gizmos.DrawSphere(points[i].position, 0.08f);
            int j = i + 1;
            if (j < points.Length && points[j] != null)
                Gizmos.DrawLine(points[i].position, points[j].position);
        }
        if (loop && points.Length > 1 && points[0] != null && points[^1] != null)
            Gizmos.DrawLine(points[^1].position, points[0].position);
    }
}
// This script defines a WaypointPath class that manages a series of waypoints in a Unity scene.