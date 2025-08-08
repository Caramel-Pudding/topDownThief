using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GuardVisionVisualizer : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private GuardPerception perception;

    [Header("Mesh")]
    [SerializeField] private int rayCount = 128;
    [SerializeField] private float edgeOffset = 0.01f;

    [Header("Render")]
    [ColorUsage(true, true)]
    [SerializeField] private Color color = new Color(1f, 1f, 0f, 0.25f);
    [SerializeField] private int sortingOrder = 200;
    [SerializeField] private string sortingLayerName = "Effects";
    [SerializeField] private string shaderName = "Universal Render Pipeline/Unlit";
    // If you prefer always-transparent: "Universal Render Pipeline/Particles/Unlit"

    private Mesh mesh;
    private MeshRenderer mr;
    private MeshFilter mf;
    private Material mat;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        mesh = new Mesh { name = "GuardFOVMesh" };
        mf.mesh = mesh;

        // Create and configure material
        var shader = Shader.Find(shaderName);
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        }
        mat = new Material(shader);
        ConfigureMaterialTransparent(mat);
        ApplyColor(mat, color);

        mr.material = mat;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;

        if (!string.IsNullOrEmpty(sortingLayerName))
            mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = sortingOrder;

        if (!perception) perception = GetComponent<GuardPerception>();
    }

    void LateUpdate()
    {
        if (perception == null || perception.config == null) return;

        Transform eyes = perception.eyes ? perception.eyes : transform;
        var cfg = perception.config;

        float radius = Mathf.Max(0.01f, cfg.visionRadius);
        float halfFov = cfg.fov * 0.5f;
        Vector2 origin = eyes.position;
        Vector2 forward = eyes.right;

        var vertices = new Vector3[rayCount + 2];
        var triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // local origin

        int vIndex = 1;
        int tIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            float t = (float)i / rayCount;
            float angle = -halfFov + cfg.fov * t;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(
                forward.x * Mathf.Cos(rad) - forward.y * Mathf.Sin(rad),
                forward.x * Mathf.Sin(rad) + forward.y * Mathf.Cos(rad)
            ).normalized;

            float dist = radius;
            var hit = Physics2D.Raycast(origin, dir, radius, cfg.obstaclesMask);
            if (hit.collider != null) dist = Mathf.Max(0f, hit.distance - edgeOffset);

            Vector2 worldPoint = origin + dir * dist;
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            vertices[vIndex] = new Vector3(localPoint.x, localPoint.y, 0f);

            if (i > 0)
            {
                triangles[tIndex++] = 0;
                triangles[tIndex++] = vIndex - 1;
                triangles[tIndex++] = vIndex;
            }

            vIndex++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    public void SetColor(Color c)
    {
        color = c;
        if (mat) ApplyColor(mat, color);
    }

    void OnValidate()
    {
        if (mr && mr.sharedMaterial) ApplyColor(mr.sharedMaterial, color);
    }

    private static void ConfigureMaterialTransparent(Material m)
    {
        // URP standard transparent setup
        if (m.HasProperty("_Surface"))
        {
            m.SetFloat("_Surface", 1f); // Transparent
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        m.DisableKeyword("_ALPHATEST_ON");
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_Cull", (float)CullMode.Off);
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        m.renderQueue = (int)RenderQueue.Transparent;
    }

    private static void ApplyColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }
}
