using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DoorController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Collider2D solidCollider;      // non-trigger barrier
    [SerializeField] private Collider2D triggerCollider;    // interact trigger (optional)
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ShadowCaster2D shadowCaster;

    [Header("Behaviour")]
    [SerializeField] private float autoCloseSeconds = 0f;

    [Header("Layers")]
    [SerializeField] private string closedLayerName = "Obstacles";
    [SerializeField] private string openLayerName = "Default";

    [Header("Events")]
    public UnityEvent OnOpened;
    public UnityEvent OnClosed;

    private bool opened;
    private bool initialRendererEnabled = true;
    private int initialLayer;
    private float autoCloseTimer;

    private int closedLayer = -1;
    private int openLayer = -1;

    public bool IsOpen => opened;

    void OnValidate()
    {
        if (!spriteRenderer) TryGetComponent(out spriteRenderer);
        if (!shadowCaster) TryGetComponent(out shadowCaster);
        EnsureCollidersAreAssigned();
    }

    private void EnsureCollidersAreAssigned()
    {
        if (solidCollider && triggerCollider) return;

        var allColliders = GetComponents<Collider2D>();

        if (!solidCollider)
        {
            foreach (var c in allColliders)
            {
                if (!c.isTrigger)
                {
                    solidCollider = c;
                    break;
                }
            }
        }

        if (!triggerCollider)
        {
            foreach (var c in allColliders)
            {
                if (c.isTrigger)
                {
                    triggerCollider = c;
                    break;
                }
            }
        }
    }

    void Awake()
    {
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) initialRendererEnabled = spriteRenderer.enabled;

        initialLayer = gameObject.layer;

        // Resolve layers early
        closedLayer = LayerMask.NameToLayer(closedLayerName);
        openLayer = LayerMask.NameToLayer(openLayerName);

        if (closedLayer < 0)
            Debug.LogError($"[DoorController] Layer '{closedLayerName}' not found. Check Project Settings > Tags and Layers.");
        if (openLayer < 0)
            Debug.LogError($"[DoorController] Layer '{openLayerName}' not found. Check Project Settings > Tags and Layers.");

        EnsureCollidersAreAssigned();

        // Fallback for solid collider
        if (!solidCollider)
        {
            var colliders = GetComponents<Collider2D>();
            if (colliders.Length > 0)
            {
                solidCollider = colliders[0];
                Debug.LogWarning($"[DoorController] No non-trigger collider found for '{gameObject.name}'. Using first available collider. It will be forced solid when closed.", this);
            }
        }

        // Apply initial visual/physics state without invoking events
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
        if (spriteRenderer) spriteRenderer.enabled = isOpen ? false : initialRendererEnabled;
        if (shadowCaster) shadowCaster.enabled = !isOpen;
        if (solidCollider)
        {
            solidCollider.enabled = !isOpen;
            if (!isOpen)
            {
                solidCollider.isTrigger = false;
            }
        }

        gameObject.layer = isOpen
            ? LayerMask.NameToLayer(openLayerName)
            : LayerMask.NameToLayer(closedLayerName);

        if (invokeEvents)
        {
            if (isOpen) OnOpened?.Invoke();
            else OnClosed?.Invoke();
        }
    }
}
