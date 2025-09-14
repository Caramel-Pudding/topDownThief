using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Loot : MonoBehaviour
{
    private InputSystem_Actions controls;

    [Header("UI")]
    public TMP_Text promptText;
    [Tooltip("Optional nice name for prompt, e.g. 'Small Key'")]
    public string displayName = "Loot";

    [Header("Interaction")]
    public string playerTag = "Player";
    [SerializeField] private float interactDebounce = 0.1f;

    [Header("Single Item (legacy)")]
    [Tooltip("Used if 'items' array is empty. Kept for backward compatibility.")]
    public string itemId = "key";
    [Tooltip("Amount for the legacy single item mode.")]
    public int amount = 1;

    [Header("Multiple Items (container mode)")]
    [Tooltip("If not empty, this loot will grant all items from the list.")]
    public ItemStack[] items;

    [Header("Behaviour")]
    [Tooltip("Destroy this GameObject after successful pickup/collect.")]
    public bool destroyOnPickup = true;

    private bool playerInside;
    private bool interactionComplete;
    private float lastInteractTime;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += OnInteractPerformed;
    }

    void OnEnable()
    {
        try { controls.Player.Enable(); } catch {}
    }

    void OnDisable()
    {
        try { controls.Player.Disable(); } catch {}
    }

    void OnDestroy()
    {
        controls.Player.Interact.performed -= OnInteractPerformed;
        controls.Dispose();
    }

    void OnTriggerEnter2D(Collider2D body)
    {
        if (body.CompareTag(playerTag))
        {
            playerInside = true;
            UpdatePrompt(true);
        }
    }

    void OnTriggerExit2D(Collider2D body)
    {
        if (body.CompareTag(playerTag))
        {
            playerInside = false;
            UpdatePrompt(false);
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext _)
    {
        if (!playerInside || interactionComplete) return;
        if (Time.unscaledTime - lastInteractTime < interactDebounce) return;
        lastInteractTime = Time.unscaledTime;

        CompleteInteraction();
    }

    /// <summary>
    /// Public API so containers (e.g., chests) can collect loot programmatically.
    /// Returns true if collection succeeded for at least one item.
    /// </summary>
    public bool CollectNow()
    {
        if (interactionComplete) return false;
        var success = GrantItems();
        if (success)
        {
            interactionComplete = true;
            UpdatePrompt(false);
            if (destroyOnPickup) Destroy(gameObject);
        }
        return success;
    }

    private void CompleteInteraction()
    {
        if (GrantItems())
        {
            interactionComplete = true;
            UpdatePrompt(false);
            if (destroyOnPickup) Destroy(gameObject);
        }
        else
        {
            // Optional: play deny SFX or feedback
        }
    }

    private bool GrantItems()
    {
        if (InventoryManager.Instance == null) return false;

        bool anyAdded = false;

        if (items != null && items.Length > 0)
        {
            foreach (var stack in items)
            {
                if (string.IsNullOrEmpty(stack.itemId) || stack.amount <= 0) continue;
                anyAdded |= AddItemSafe(stack.itemId, stack.amount);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(itemId) && amount > 0)
                anyAdded |= AddItemSafe(itemId, amount);
        }

        return anyAdded;
    }

    // Compatibility helper: if your InventoryManager doesn't have AddItem(id, amount),
    // this fallback loops single-adds. No need to change InventoryManager urgently.
    private bool AddItemSafe(string id, int count)
    {
        bool added = false;

        // If you DO have AddItem(string, int), uncomment and prefer the direct call:
        // added |= InventoryManager.Instance.AddItem(id, count);

        // Generic fallback: call AddItem(string) multiple times
        for (int i = 0; i < count; i++)
        {
            // Try legacy signature
            added |= InventoryManager.Instance.AddItem(id);
        }

        return added;
    }

    private void UpdatePrompt(bool show)
    {
        if (!promptText) return;

        if (!show || interactionComplete)
        {
            promptText.enabled = false;
            return;
        }

        string keyName;
        try { keyName = controls.Player.Interact.GetBindingDisplayString(); }
        catch { keyName = "Interact"; }

        var label = !string.IsNullOrEmpty(displayName) ? displayName : itemId;
        promptText.text = $"Press {keyName} to loot {label}";
        promptText.enabled = true;
    }
}
