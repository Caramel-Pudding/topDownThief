using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class DashWindVFX : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private SpriteRenderer sourceSprite;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private ParticleSystem dashBurst;

    [Header("Afterimages")]
    [SerializeField] private float afterimageInterval = 0.02f;
    [SerializeField] private float afterimageLifetime = 0.15f;
    [SerializeField] private Color afterimageColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Material afterimageMaterial; // optional; if null, uses source material

    ParticleSystem dashStreaks;

    private PlayerMovement movement;
    private Coroutine afterimageRoutine;

    void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        sourceSprite = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();
    }

    void Awake()
    {
        if (!body) body = GetComponent<Rigidbody2D>();
        if (!movement) movement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        movement.OnDashStart += HandleDashStart;
        movement.OnDashEnd += HandleDashEnd;
        if (trail) trail.emitting = false;
    }

    void OnDisable()
    {
        movement.OnDashStart -= HandleDashStart;
        movement.OnDashEnd -= HandleDashEnd;
    }

    private void HandleDashStart(Vector2 dir)
    {
        if (trail)
        {
            trail.textureMode = LineTextureMode.Tile;
            trail.time = movement.dashDuration * 1.1f;
            trail.emitting = true;
        }

        // Короткий всплеск (как было)
        if (dashBurst)
        {
            dashBurst.transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);
            dashBurst.Play(true);
        }

        // Включаем «штрихи»
        if (dashStreaks)
        {
            var emission = dashStreaks.emission;
            emission.rateOverTime = 300f; // активный поток
            var vel = dashStreaks.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-dir.x * 6f);
            vel.y = new ParticleSystem.MinMaxCurve(-dir.y * 6f);
            dashStreaks.Play(true);
        }

        if (afterimageRoutine != null) StopCoroutine(afterimageRoutine);
        afterimageRoutine = StartCoroutine(SpawnAfterimages());
    }

    private void HandleDashEnd(Vector2 dir)
    {
        if (trail) trail.emitting = false;

        if (dashStreaks)
        {
            // мгновенно выключаем поток, но даём частицам дожить свой lifetime
            var emission = dashStreaks.emission;
            emission.rateOverTime = 0f;
        }

        if (afterimageRoutine != null)
        {
            StopCoroutine(afterimageRoutine);
            afterimageRoutine = null;
        }
    }

    private IEnumerator SpawnAfterimages()
    {
        var wait = new WaitForSeconds(afterimageInterval);
        while (true)
        {
            if (sourceSprite && sourceSprite.sprite)
                CreateAfterimage();
            yield return wait;
        }
    }

    private void CreateAfterimage()
    {
        var go = new GameObject("Afterimage");
        go.transform.position = sourceSprite.transform.position;
        go.transform.rotation = sourceSprite.transform.rotation;
        go.transform.localScale = sourceSprite.transform.lossyScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sourceSprite.sprite;
        sr.flipX = sourceSprite.flipX;
        sr.flipY = sourceSprite.flipY;
        sr.sortingLayerID = sourceSprite.sortingLayerID;
        sr.sortingOrder = sourceSprite.sortingOrder - 1;
        sr.material = afterimageMaterial ? afterimageMaterial : sourceSprite.sharedMaterial;
        sr.color = afterimageColor;

        var fader = go.AddComponent<AfterimageFader>();
        fader.Init(afterimageLifetime);
    }
}

public class AfterimageFader : MonoBehaviour
{
    private SpriteRenderer sr;
    private float lifetime;
    private float t;

    public void Init(float life)
    {
        lifetime = Mathf.Max(0.01f, life);
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!sr) { Destroy(gameObject); return; }
        t += Time.deltaTime;
        float k = 1f - Mathf.Clamp01(t / lifetime);
        var c = sr.color;
        c.a = k * c.a;
        sr.color = c;
        if (t >= lifetime) Destroy(gameObject);
    }
}
