using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DoorController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Collider2D solidCollider;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ShadowCaster2D shadowCaster;

    [Header("Behaviour")]
    [SerializeField] private float autoCloseSeconds = 0f;

    [Header("Events")]
    public UnityEvent OnOpened;
    public UnityEvent OnClosed;

    private bool opened;
    private bool initialRendererEnabled = true;
    private int initialLayer;
    private float autoCloseTimer;

    public bool IsOpen => opened;

    void OnValidate()
    {
        if (!spriteRenderer) TryGetComponent(out spriteRenderer);
        if (!shadowCaster) TryGetComponent(out shadowCaster);

        if (!solidCollider)
        {
            foreach (var c in GetComponents<Collider2D>())
                if (!c.isTrigger) { solidCollider = c; break; }
        }

        if (!triggerCollider)
        {
            foreach (var c in GetComponents<Collider2D>())
                if (c.isTrigger) { triggerCollider = c; break; }
        }
    }

    void Awake()
    {
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) initialRendererEnabled = spriteRenderer.enabled;
        initialLayer = gameObject.layer;

        if (!solidCollider) OnValidate();
        ApplyState(opened, invokeEvents: false);
    }

    void Update()
    {
        if (!opened || autoCloseSeconds <= 0f) return;
        autoCloseTimer -= Time.deltaTime;
        if (autoCloseTimer <= 0f) Close();
    }

    public void Open()
    {
        if (opened) return;
        opened = true;
        autoCloseTimer = autoCloseSeconds;
        ApplyState(opened, invokeEvents: true);
    }

    public void Close()
    {
        if (!opened) return;
        opened = false;
        ApplyState(opened, invokeEvents: true);
    }

    public void Toggle()
    {
        if (opened) Close(); else Open();
    }

    private void ApplyState(bool isOpen, bool invokeEvents)
    {
        if (spriteRenderer) spriteRenderer.enabled = !isOpen ? initialRendererEnabled : false;
        if (solidCollider) solidCollider.enabled = !isOpen;
        if (shadowCaster) shadowCaster.enabled = !isOpen;
        gameObject.layer = isOpen ? gameObject.layer : initialLayer;

        if (invokeEvents)
        {
            if (isOpen) OnOpened?.Invoke();
            else OnClosed?.Invoke();
        }
    }
}
