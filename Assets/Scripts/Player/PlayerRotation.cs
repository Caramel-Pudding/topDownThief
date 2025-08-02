using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteMask))]
public class PlayerWithFOV : MonoBehaviour
{
    [Header("FOV Settings")]
    public float viewAngle = 90f;
    public float viewRadius = 8f;
    public int texSize = 128;
    public int rayCount = 128;
    public LayerMask obstacleMask;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteHolder;
    [SerializeField] private float offsetAngle = 0f;

    private SpriteMask spriteMask;
    private Texture2D fovTexture;
    private Sprite fovSprite;
    private Camera mainCam;
    private float camZDistance;
    private Animator animator;
    private float mouseAngle;

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

        mainCam = Camera.main;
        camZDistance = Mathf.Abs(mainCam.transform.position.z);

        if (spriteHolder != null)
            animator = spriteHolder.GetComponent<Animator>();
    }

    void Update()
    {
        HandleRotationAndAnimation();
    }

    void LateUpdate()
    {
        GenerateFOVTexture();
        fovTexture.Apply();
    }

    void HandleRotationAndAnimation()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, camZDistance)
        );

        Vector2 direction = (mouseWorld - transform.position).normalized;

        if (spriteHolder != null)
            spriteHolder.flipX = direction.x < 0f;

        if (animator != null)
        {
            animator.SetFloat("dirX", direction.x);
            animator.SetFloat("dirY", direction.y);
        }

        // Запоминаем угол направления взгляда (в градусах)
        mouseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + offsetAngle;
    }

    void GenerateFOVTexture()
    {
        Color32[] pixels = new Color32[texSize * texSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 0);

        // 8f is a char height
        Vector2 center = new Vector2(texSize / 2f, texSize / 2f + 8f);
        float pixelPerUnit = texSize / (viewRadius * 2f);

        float startAngle = mouseAngle - viewAngle / 2f;
        float angleStep = viewAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = Mathf.Deg2Rad * angle;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 worldCenter = (Vector2)transform.position;

            bool wasInObstacle = false;

            for (float r = 0f; r < viewRadius; r += 0.05f)
            {
                Vector2 samplePos = worldCenter + dir * r;
                bool isObstacle = Physics2D.OverlapPoint(samplePos, obstacleMask);

                // Пока не был в obstacle — просто идём вперёд
                // Если вошли в obstacle — продолжаем идти (заливка продолжается)
                // Как только вышли из obstacle (wasInObstacle==true && isObstacle==false) — break
                if (wasInObstacle && !isObstacle)
                    break;

                Vector2 pos = center + dir * r * pixelPerUnit;
                int px = Mathf.RoundToInt(pos.x);
                int py = Mathf.RoundToInt(pos.y);
                if (px >= 0 && px < texSize && py >= 0 && py < texSize)
                    pixels[py * texSize + px] = new Color32(255, 255, 255, 255);

                if (isObstacle)
                    wasInObstacle = true;
            }
        }

        fovTexture.SetPixels32(pixels);
    }


}
