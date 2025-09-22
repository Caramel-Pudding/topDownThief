using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Audio;

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

    [Header("Audio")]
    [SerializeField] private AudioClip pickupClip;   // played on successful pickup
    [SerializeField] private AudioClip denyClip;     // played if pickup failed
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private AudioMixerGroup outputGroup; // optional
    [SerializeField] private bool twoDSound = true;  // 2D ensures consistent volume
    private AudioSource audioSource;

    private bool playerInside;
    private bool interactionComplete;
    private float lastInteractTime;
    private Collider2D cachedCollider;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += OnInteractPerformed;

        cachedCollider = GetComponent<Collider2D>();
        EnsureAudioSource();
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

        bool success = GrantItems();
        HandlePostGrant(success);
        return success;
    }

    private void CompleteInteraction()
    {
        bool success = GrantItems();
        HandlePostGrant(success);
    }

    private void HandlePostGrant(bool success)
    {
        if (success)
        {
            interactionComplete = true;
            UpdatePrompt(false);

            // Prevent re-trigger while sound plays
            if (cachedCollider) cachedCollider.enabled = false;

            float delay = 0f;
            if (pickupClip)
            {
                audioSource.PlayOneShot(pickupClip, sfxVolume);
                delay = pickupClip.length;
            }

            if (destroyOnPickup)
            {
                // Hide visuals immediately but let the sound finish
                HideVisualsIfAny();
                Destroy(gameObject, Mathf.Max(0.01f, delay));
            }
        }
        else
        {
            if (denyClip) audioSource.PlayOneShot(denyClip, sfxVolume);
            // Keep object; player may try again (e.g., inventory full logic if you add it later)
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

        // If you DO have AddItem(string, int), prefer the direct call:
        // added |= InventoryManager.Instance.AddItem(id, count);

        for (int i = 0; i < count; i++)
        {
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

    private void EnsureAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = twoDSound ? 0f : 1f;
        if (outputGroup) audioSource.outputAudioMixerGroup = outputGroup;
        // Other defaults (volume/pitch) are controlled via PlayOneShot and sfxVolume
    }

    private void HideVisualsIfAny()
    {
        // Optional: disable renderers so loot disappears immediately
        var renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = false;

        if (promptText) promptText.enabled = false;
    }
}
