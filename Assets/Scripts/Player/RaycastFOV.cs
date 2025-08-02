using UnityEngine;

[RequireComponent(typeof(SpriteMask))]
public class RaycastFOV : MonoBehaviour
{
    [Header("FOV Settings")]
    public float viewAngle = 90f;
    public float viewRadius = 8f;
    public int texSize = 128;
    public int rayCount = 128;
    public LayerMask obstacleMask;

    private SpriteMask spriteMask;
    private Texture2D fovTexture;
    private Sprite fovSprite;

    void Awake()
    {
        spriteMask = GetComponent<SpriteMask>();
        fovTexture = new Texture2D(texSize, texSize, TextureFormat.Alpha8, false);
        fovTexture.filterMode = FilterMode.Point;

        fovSprite = Sprite.Create(
            fovTexture,
            new Rect(0, 0, texSize, texSize),
            new Vector2(0.5f, 0.5f),
            texSize / (viewRadius * 2f)
        );

        spriteMask.sprite = fovSprite;
    }

    void LateUpdate()
    {
        GenerateFOVTexture();
        fovTexture.Apply();
        // Если маска вдруг не обновляется:
        // spriteMask.enabled = false; spriteMask.enabled = true;
    }

    void GenerateFOVTexture()
    {
        Color32[] pixels = new Color32[texSize * texSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 0);

        Vector2 center = new Vector2(texSize / 2f, texSize / 2f);
        float pixelPerUnit = texSize / (viewRadius * 2f);

        float startAngle = -viewAngle / 2f;
        float angleStep = viewAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i + transform.eulerAngles.z;
            float rad = Mathf.Deg2Rad * angle;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 worldCenter = (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(worldCenter, dir, viewRadius, obstacleMask);
            float hitDist = hit ? hit.distance : viewRadius;

            for (float r = 0; r < hitDist; r += 0.1f)
            {
                Vector2 pos = center + dir * r * pixelPerUnit;
                int px = Mathf.RoundToInt(pos.x);
                int py = Mathf.RoundToInt(pos.y);
                if (px >= 0 && px < texSize && py >= 0 && py < texSize)
                    pixels[py * texSize + px] = new Color32(255, 255, 255, 255);
            }
        }

        fovTexture.SetPixels32(pixels);
    }
}
