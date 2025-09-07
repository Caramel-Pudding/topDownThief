using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DoorController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Collider2D solidCollider;      // non-trigger barrier
    [SerializeField] private Collider2D triggerCollider;    // optional trigger for interact
    [SerializeField] private SpriteRenderer spriteRenderer; // door visuals
    [SerializeField] private ShadowCaster2D shadowCaster;   // optional

    private bool opened;
    private bool initialRendererEnabled = true;
    private int initialLayer;

    public bool IsOpen => opened;

    void OnValidate()
    {
        if (!spriteRenderer) TryGetComponent(out spriteRenderer);
        if (!shadowCaster) TryGetComponent(out shadowCaster);

        // Auto-pick first non-trigger as solid, first trigger as trigger
        if (!solidCollider)
            foreach (var c in GetComponents<Collider2D>())
                if (!c.isTrigger) { solidCollider = c; break; }

        if (!triggerCollider)
            foreach (var c in GetComponents<Collider2D>())
                if (c.isTrigger) { triggerCollider = c; break; }
    }

    void Awake()
    {
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) initialRendererEnabled = spriteRenderer.enabled;
        initialLayer = gameObject.layer;

        if (!solidCollider) OnValidate(); // last-chance autowire
    }

    public void Open()
    {
        if (opened) return;
        opened = true;

        if (spriteRenderer) spriteRenderer.enabled = false; // hide door
        if (solidCollider) solidCollider.enabled = false;   // pass-through
        if (shadowCaster) shadowCaster.enabled = false;     // no shadows

        // оставляем триггер включённым, чтобы можно было закрыть дверь тем же Interact
    }

    public void Close()
    {
        if (!opened) return;
        opened = false;

        if (spriteRenderer) spriteRenderer.enabled = initialRendererEnabled;
        if (solidCollider) solidCollider.enabled = true;
        if (shadowCaster) shadowCaster.enabled = true;

        gameObject.layer = initialLayer;
    }

    public void Toggle()
    {
        if (opened) Close(); else Open();
    }
}
