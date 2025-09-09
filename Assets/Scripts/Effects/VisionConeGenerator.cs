using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionConeMesh : MonoBehaviour
{
    public float fov = 90f;
    public float radius = 5f;
    public int segments = 20;

    void Start()
    {
        UpdateVision();
    }

    void UpdateVision()
    {
        // --- 1. Обновляем PolygonCollider2D ---
        var poly = GetComponent<PolygonCollider2D>();
        Vector2[] points = new Vector2[segments + 2];
        points[0] = Vector2.zero; // центр

        for (int i = 0; i <= segments; i++)
        {
            float angle = -fov / 2f + (fov * i / segments);
            float rad = Mathf.Deg2Rad * angle;
            points[i + 1] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }
        poly.points = points;

        // --- 2. Обновляем Mesh ---
        var mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];
        vertices[0] = Vector3.zero;
        for (int i = 0; i <= segments; i++)
            vertices[i + 1] = points[i + 1];

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        var mf = GetComponent<MeshFilter>();
        mf.mesh = mesh;
    }
}
