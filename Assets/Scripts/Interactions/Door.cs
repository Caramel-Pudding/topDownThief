using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DoorTrigger : MonoBehaviour
{
    [Header("Lock")]
    [SerializeField] private string keyId = "key";
    [SerializeField] private bool consumeKey = true;

    [Header("Player & UI")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private TMP_Text promptText;

    [Header("Visuals")]
    [SerializeField] private Sprite openDoorSprite;

    private InputSystem_Actions controls;
    private bool playerInside;
    private bool doorOpened;

    private Collider2D triggerCollider;
    private Collider2D solidCollider;
    private ShadowCaster2D shadowCaster;
    private SpriteRenderer doorSprite;

    void Reset()
    {
        // Cache common components when adding the script
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        doorSprite = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        solidCollider = GetComponent<Collider2D>(); // will be overwritten in Awake by the non-trigger one if present
        shadowCaster = GetComponent<ShadowCaster2D>();
    }

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += OnInteract;

        triggerCollider = GetComponent<Collider2D>();

        // Find the solid (non-trigger) collider on the same GameObject
        foreach (var col in GetComponents<Collider2D>())
        {
            if (!col.isTrigger)
            {
                solidCollider = col;
                break;
            }
        }

        shadowCaster = shadowCaster ?? GetComponent<ShadowCaster2D>();
        doorSprite = doorSprite ?? GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
    }

    void OnEnable()
    {
        // Enable only while player is inside to avoid many doors listening at once
        if (playerInside) controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void OnDestroy()
    {
        controls.Player.Interact.performed -= OnInteract;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (doorOpened || !other.CompareTag(playerTag)) return;
        playerInside = true;
        controls.Player.Enable();
        UpdatePrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        controls.Player.Disable();
        SetPromptVisible(false);
    }

    private void OnInteract(InputAction.CallbackContext _)
    {
        if (!playerInside || doorOpened) return;

        if (InventoryManager.Instance.HasItem(keyId))
        {
            if (consumeKey) InventoryManager.Instance.RemoveItem(keyId);
            OpenDoor();
        }
        else
        {
            // Optional: flash "requires key"
            UpdatePrompt();
        }
    }

    private void UpdatePrompt()
    {
        if (promptText == null) return;

        if (doorOpened)
        {
            SetPromptVisible(false);
            return;
        }

        if (InventoryManager.Instance.HasItem(keyId))
        {
            // Try to display the first binding of Interact in a readable form
            string keyName = controls.Player.Interact.GetBindingDisplayString();
            promptText.text = $"Press {keyName} to open door";
        }
        else
        {
            promptText.text = "Door requires a key";
        }

        SetPromptVisible(true);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText != null) promptText.enabled = visible;
    }

    private void OpenDoor()
    {
        if (doorOpened) return;

        doorOpened = true;
        SetPromptVisible(false);

        if (openDoorSprite != null && doorSprite != null)
            doorSprite.sprite = openDoorSprite;

        if (solidCollider != null)
            solidCollider.enabled = false;

        if (shadowCaster != null)
            shadowCaster.enabled = false;

        if (triggerCollider != null)
            triggerCollider.enabled = false;

        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}
