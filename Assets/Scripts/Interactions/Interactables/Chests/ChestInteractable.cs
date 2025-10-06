using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class ChestInteractable : MonoBehaviour, IInteractable
{
    public enum AccessMode { Unlocked, Pickable, KeyRequired }

    [Header("Refs")]
    [SerializeField] private ChestController chest;
    [SerializeField] private LockComponent lockComponent; // used in Pickable/KeyRequired

    [Header("Mode")]
    [SerializeField] private AccessMode mode = AccessMode.Unlocked;

    [Header("Key Settings")]
    [SerializeField] private string requiredKeyId;
    [SerializeField] private string keyDisplayName;
    [SerializeField] private bool consumeKey = false;
    [SerializeField] private bool allowPickIfNoKey = false;

    [Header("Loot")]
    [SerializeField] private ChestLootConfig lootConfig;
    [SerializeField] private bool autoLootOnOpen = true; // if true, loot on first open

    [Header("UI")]
    [SerializeField] private TMP_Text promptText;

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool allowClose = false;
    [SerializeField] private float interactDebounce = 0.1f;

    [Header("Dependencies")]
    [SerializeField] private LockpickingInitiator lockpickingInitiator;

    [Header("Persistence")]
    [SerializeField] private string saveKeyOpened = ""; // e.g. "chest_A1_opened"
    [SerializeField] private string saveKeyLooted = ""; // e.g. "chest_A1_looted"

    private InputSystem_Actions controls;
    private bool playerInside;
    private bool IsMinigameActive => lockpickingInitiator != null && lockpickingInitiator.IsMinigameActive;
    private float lastInteractTime;
    private bool permanentlyUnlocked; // behaves like for doors

    private ISaveStore saveStore;

    private bool IsEffectivelyUnlocked =>
        permanentlyUnlocked ||
        mode == AccessMode.Unlocked ||
        (lockComponent != null && !lockComponent.IsLocked);

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnValidate()
    {
        if (!chest) TryGetComponent(out chest);
        if (!lockComponent) TryGetComponent(out lockComponent);
        if (!lockpickingInitiator) TryGetComponent(out lockpickingInitiator);
    }

    void Awake()
    {
        if (!chest) chest = GetComponent<ChestController>();
        if (!lockComponent) lockComponent = GetComponent<LockComponent>();
        if (!lockpickingInitiator) lockpickingInitiator = GetComponent<LockpickingInitiator>();

        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += OnInteract;

        saveStore = new PlayerPrefsSaveStore();
        LoadPersistentState();
        UpdatePrompt();
    }

    void OnEnable()
    {
        try { if (playerInside) controls.Player.Enable(); } catch {}
        UpdatePrompt();
    }

    void OnDisable()
    {
        try { controls.Player.Disable(); } catch {}
    }

    void OnDestroy()
    {
        controls.Player.Interact.performed -= OnInteract;
        controls.Dispose();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        try { controls.Player.Enable(); } catch {}
        UpdatePrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        try { controls.Player.Disable(); } catch {}
        SetPromptVisible(false);

        if (IsMinigameActive)
        {
            lockpickingInitiator.CancelLockpicking();
        }
    }

    private void OnInteract(InputAction.CallbackContext _)
    {
        if (!playerInside || IsMinigameActive) return;
        if (Time.unscaledTime - lastInteractTime < interactDebounce) return;
        lastInteractTime = Time.unscaledTime;

        // Already open
        if (chest.IsOpen)
        {
            if (!chest.IsLooted && lootConfig && autoLootOnOpen)
            {
                TryLoot();
            }
            else if (allowClose)
            {
                chest.Close();
                UpdatePrompt();
            }
            return;
        }

        // Closed -> try to open depending on access mode
        if (IsEffectivelyUnlocked)
        {
            OpenAndMaybeLoot();
            return;
        }

        switch (mode)
        {
            case AccessMode.Unlocked:
                OpenAndMaybeLoot();
                break;

            case AccessMode.Pickable:
                HandlePickable();
                break;

            case AccessMode.KeyRequired:
                HandleKeyRequired();
                break;
        }
    }

    private void HandlePickable()
    {
        if (lockComponent != null && lockComponent.IsLocked)
        {
            StartLockpicking();
        }
        else
        {
            OpenAndMaybeLoot();
        }
    }

    private void HandleKeyRequired()
    {
        if (IsEffectivelyUnlocked)
        {
            OpenAndMaybeLoot();
            return;
        }

        bool hasKey = HasKey();
        if (hasKey)
        {
            if (consumeKey) InventoryManager.Instance?.RemoveItem(requiredKeyId);
            lockComponent?.ForceUnlock();
            SetPermanentlyUnlocked(true, alsoSwitchModeToUnlocked: true);
            OpenAndMaybeLoot();
            return;
        }

        if (allowPickIfNoKey && lockComponent != null)
        {
            if (lockComponent.IsLocked)
            {
                StartLockpicking();
                return;
            }
            OpenAndMaybeLoot();
            return;
        }

        UpdatePrompt(); // show requirement
    }

    private void StartLockpicking()
    {
        if (lockpickingInitiator == null)
        {
            Debug.LogWarning("Lockpicking initiator missing.");
            return;
        }

        SetPromptVisible(false);

        var diff = lockComponent ? lockComponent.Difficulty : LockDifficulty.Medium;

        lockpickingInitiator.StartLockpicking(diff, success =>
        {
            if (success)
            {
                lockComponent?.ForceUnlock();
                SetPermanentlyUnlocked(true, alsoSwitchModeToUnlocked: true);
                OpenAndMaybeLoot();
            }
            else
            {
                UpdatePrompt();
            }
        });
    }

    private void OpenAndMaybeLoot()
    {
        chest.Open();
        SaveOpened(true);

        if (autoLootOnOpen && lootConfig && !chest.IsLooted)
        {
            TryLoot();
        }
        UpdatePrompt();
    }

    private void TryLoot()
    {
        if (!lootConfig || lootConfig.items == null || lootConfig.items.Length == 0)
        {
            chest.SetLooted(true);
            SaveLooted(true);
            UpdatePrompt();
            return;
        }

        bool anyAdded = false;
        foreach (var stack in lootConfig.items)
        {
            if (string.IsNullOrEmpty(stack.itemId) || stack.amount <= 0) continue;
            anyAdded |= InventoryManager.Instance?.AddItem(stack.itemId, stack.amount) ?? false;
        }

        chest.SetLooted(true);
        SaveLooted(true);
        UpdatePrompt();
    }

    private void UpdatePrompt()
    {
        if (!promptText) return;

        string keyName = SafeBindingDisplay();
        string keyLabel = string.IsNullOrEmpty(keyDisplayName) ? requiredKeyId : keyDisplayName;

        if (chest.IsOpen)
        {
            if (!chest.IsLooted && lootConfig)
            {
                promptText.text = autoLootOnOpen
                    ? $"Press {keyName} to open"
                    : $"Press {keyName} to loot";
                // Note: if autoLootOnOpen=true, лут случится при открытии; здесь подсказка может быть не нужна
            }
            else
            {
                promptText.text = allowClose ? $"Press {keyName} to close" : string.Empty;
            }
            SetPromptVisible(!string.IsNullOrEmpty(promptText.text));
            return;
        }

        if (IsEffectivelyUnlocked)
        {
            promptText.text = $"Press {keyName} to open";
            SetPromptVisible(true);
            return;
        }

        switch (mode)
        {
            case AccessMode.Unlocked:
                promptText.text = $"Press {keyName} to open";
                SetPromptVisible(true);
                break;

            case AccessMode.Pickable:
                promptText.text = $"Press {keyName} to pick lock";
                SetPromptVisible(!IsMinigameActive);
                break;

            case AccessMode.KeyRequired:
                if (HasKey())
                {
                    promptText.text = consumeKey
                        ? $"Press {keyName} to use {keyLabel} and open"
                        : $"Press {keyName} to open (has {keyLabel})";
                    SetPromptVisible(true);
                }
                else if (allowPickIfNoKey && lockComponent != null)
                {
                    promptText.text = $"Press {keyName} to pick lock (missing {keyLabel})";
                    SetPromptVisible(!IsMinigameActive);
                }
                else
                {
                    promptText.text = $"{keyLabel} required";
                    SetPromptVisible(true);
                }
                break;
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText) promptText.enabled = visible;
    }

    private bool HasKey()
    {
        return !string.IsNullOrEmpty(requiredKeyId)
               && InventoryManager.Instance != null
               && InventoryManager.Instance.HasItem(requiredKeyId);
    }

    private string SafeBindingDisplay()
    {
        try { return controls.Player.Interact.GetBindingDisplayString(); }
        catch { return "Interact"; }
    }

    private void LoadPersistentState()
    {
        if (saveStore == null) saveStore = new PlayerPrefsSaveStore();

        if (!string.IsNullOrEmpty(saveKeyOpened) &&
            saveStore.TryGetBool(saveKeyOpened, out var opened))
        {
            if (opened) chest.Open();
        }

        if (!string.IsNullOrEmpty(saveKeyLooted) &&
            saveStore.TryGetBool(saveKeyLooted, out var looted))
        {
            chest.SetLooted(looted);
        }

        // If chest was opened previously, we also consider it "permanentlyUnlocked"
        if (chest.IsOpen)
        {
            permanentlyUnlocked = true;
            if (lockComponent) lockComponent.ForceUnlock();
            mode = AccessMode.Unlocked;
        }
    }

    private void SaveOpened(bool value)
    {
        if (!string.IsNullOrEmpty(saveKeyOpened))
            saveStore.SetBool(saveKeyOpened, value);
    }

    private void SaveLooted(bool value)
    {
        if (!string.IsNullOrEmpty(saveKeyLooted))
            saveStore.SetBool(saveKeyLooted, value);
    }

    private void SetPermanentlyUnlocked(bool value, bool alsoSwitchModeToUnlocked)
    {
        permanentlyUnlocked = value;
        if (lockComponent && value) lockComponent.ForceUnlock();
        if (alsoSwitchModeToUnlocked) mode = AccessMode.Unlocked;
    }

    // IInteractable
    public bool CanInteract(object actor) => playerInside && !IsMinigameActive;
    public void Interact(object actor) => OnInteract(default);
    public string GetPrompt(object actor) => promptText ? promptText.text : string.Empty;


}
