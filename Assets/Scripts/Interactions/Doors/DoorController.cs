using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DoorController : MonoBehaviour
{
    [Header("Colliders & Visuals")]
    [SerializeField] private Collider2D solidCollider;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private ShadowCaster2D shadowCaster;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openSprite;

    private Sprite initialSprite;
    private int initialLayer;
    private bool opened;

    public bool IsOpen => opened;

    void Awake()
    {
        if (!triggerCollider) triggerCollider = GetComponent<Collider2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!solidCollider)
        {
            foreach (var c in GetComponents<Collider2D>())
                if (!c.isTrigger) { solidCollider = c; break; }
        }

        if (spriteRenderer) initialSprite = spriteRenderer.sprite;
        initialLayer = gameObject.layer;
    }

    public void Open()
    {
        if (opened) return;
        opened = true;

        if (spriteRenderer && openSprite) spriteRenderer.sprite = openSprite;
        if (solidCollider) solidCollider.enabled = false;
        if (shadowCaster) shadowCaster.enabled = false;

        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void Close()
    {
        if (!opened) return;
        opened = false;

        if (spriteRenderer) spriteRenderer.sprite = initialSprite;
        if (solidCollider) solidCollider.enabled = true;
        if (shadowCaster) shadowCaster.enabled = true;

        gameObject.layer = initialLayer;
    }

    public void Toggle()
    {
        if (opened) Close(); else Open();
    }
}
