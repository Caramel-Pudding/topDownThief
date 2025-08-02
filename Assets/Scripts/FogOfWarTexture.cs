using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FogOfWarTexture : MonoBehaviour
{
    [Header("Map")]
    public Vector2 worldSize = new(100, 100);
    public int texSize = 512;

    [Header("Fog Colors")]
    public float greyAlpha = 0.4f;

    Texture2D fogTex;
    Color32[] revealPixels;
    bool[] everRevealed;
    float pxPerUnit;

    SpriteRenderer fogRenderer;

    void Awake()
    {
        fogRenderer = GetComponent<SpriteRenderer>();

        fogTex = new Texture2D(texSize, texSize, TextureFormat.Alpha8, false);
        fogTex.filterMode = FilterMode.Point;
        revealPixels = new Color32[texSize * texSize];
        everRevealed = new bool[texSize * texSize];
        pxPerUnit = texSize / worldSize.x;

        // Создаём спрайт из текстуры
        var spr = Sprite.Create(
            fogTex,
            new Rect(0, 0, texSize, texSize),
            new Vector2(0.5f, 0.5f),
            texSize / worldSize.x
        );
        fogRenderer.sprite = spr;
        fogRenderer.transform.localPosition = Vector3.zero;
        fogRenderer.transform.localScale = Vector3.one;
        fogRenderer.color = Color.white;
        // Для простоты, сразу Unlit-материал
        var unlit = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        fogRenderer.material = new Material(unlit);
    }

    // Сброс временной маски видимости
    public void ClearVisibleMask()
    {
        for (int i = 0; i < revealPixels.Length; i++)
            revealPixels[i].a = 0;
    }

    // Пометить область как «видимую сейчас» и навсегда «разведанную»
    public void MarkVisible(Vector2 worldPos, float radius)
    {
        Vector2 center = WorldToTex(worldPos);
        int rad = Mathf.CeilToInt(radius * pxPerUnit);

        for (int dy = -rad; dy <= rad; dy++)
            for (int dx = -rad; dx <= rad; dx++)
            {
                int px = Mathf.RoundToInt(center.x) + dx;
                int py = Mathf.RoundToInt(center.y) + dy;
                if (px < 0 || px >= texSize || py < 0 || py >= texSize) continue;
                if (dx * dx + dy * dy > rad * rad) continue;

                int idx = py * texSize + px;
                revealPixels[idx].a = 255;         // видно сейчас
                everRevealed[idx] = true;          // разведано навсегда
            }
    }

    // В конце кадра: обновить текстуру по состоянию
    public void FinalizeFrame()
    {
        Color32[] visualPixels = new Color32[revealPixels.Length];
        for (int i = 0; i < visualPixels.Length; i++)
        {
            if (revealPixels[i].a == 255)
                visualPixels[i] = new Color32(0, 0, 0, 0); // видно — полностью прозрачный
            else if (everRevealed[i])
                visualPixels[i] = new Color32(0, 0, 0, (byte)(greyAlpha * 255f)); // разведано — полупрозрачный серый
            else
                visualPixels[i] = new Color32(0, 0, 0, 255); // не разведано — чёрный, непрозрачный
        }
        fogTex.SetPixels32(visualPixels);
        fogTex.Apply();
    }

    // Перевод мировых координат в координаты текстуры
    Vector2 WorldToTex(Vector2 worldPos)
    {
        float x = (worldPos.x / worldSize.x + 0.5f) * texSize;
        float y = (worldPos.y / worldSize.y + 0.5f) * texSize;
        return new Vector2(x, y);
    }
}
