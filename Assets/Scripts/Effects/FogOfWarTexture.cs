using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FogOfWarTexture : MonoBehaviour
{
    public Vector2 worldSize = new(100, 100);
    public int texSize = 512;

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
    }

    // Сброс временной маски видимости
    public void ClearVisibleMask()
    {
        for (int i = 0; i < revealPixels.Length; i++)
            revealPixels[i].a = 0;
    }

    public void MarkVisible(Color32[] visiblePixels, Vector2 fovCenterWorld, float fovRadius, int fovTexSize)
    {
        float fovPxPerUnit = fovTexSize / (fovRadius * 2f);

        // Центр FOV текстуры в локальных координатах
        Vector2 fovTexCenter = new Vector2(fovTexSize / 2f, fovTexSize / 2f);

        for (int py = 0; py < fovTexSize; py++)
        {
            for (int px = 0; px < fovTexSize; px++)
            {
                int idx = py * fovTexSize + px;
                if (visiblePixels[idx].a > 0)
                {
                    // Получаем позицию в world-координатах относительно центра FOV
                    Vector2 offset = (new Vector2(px, py) - fovTexCenter) / fovPxPerUnit;
                    Vector2 worldPos = fovCenterWorld + offset;

                    // Переводим worldPos в координаты глобальной карты
                    Vector2 fogTexPos = WorldToTex(worldPos);
                    int fogPx = Mathf.FloorToInt(fogTexPos.x);
                    int fogPy = Mathf.FloorToInt(fogTexPos.y);
                    if (fogPx < 0 || fogPx >= texSize || fogPy < 0 || fogPy >= texSize)
                        continue;
                    int fogIdx = fogPy * texSize + fogPx;

                    revealPixels[fogIdx].a = 255;
                    everRevealed[fogIdx] = true;
                }
            }
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
