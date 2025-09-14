using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class DoorInteractable : MonoBehaviour, IInteractable
{
    public enum DoorAccessMode { Unlocked, Pickable, KeyRequired }

    [Header("Refs")]
    [SerializeField] private DoorController door;
    [SerializeField] private LockComponent lockComponent;

    [Header("Mode")]
    [SerializeField] private DoorAccessMode mode = DoorAccessMode.Unlocked;

    [Header("Key Settings")]
    [SerializeField] private string requiredKeyId;
    [SerializeField] private string keyDisplayName;
    [SerializeField] private bool consumeKey = false;
    [SerializeField] private bool allowPickIfNoKey = false;

    [Header("UI")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Transform uiAnchor;
    [SerializeField] private LockpickingUI lockpickingPrefab;

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool allowClose = true;
    [SerializeField] private float interactDebounce = 0.1f;

    [Header("On Fail Noise")]
    [SerializeField] private float failNoiseRadius = 30f;

    [Header("Persistence")]
    [SerializeField] private string saveKey = ""; // e.g. "door_A1_unlocked"

    private InputSystem_Actions controls;
    private bool playerInside;
    private bool minigameActive;
    private bool permanentlyUnlocked;
    private float lastInteractTime;

    private ISaveStore saveStore;

    private bool IsEffectivelyUnlocked =>
        permanentlyUnlocked ||
        mode == DoorAccessMode.Unlocked ||
        (lockComponent != null && !lockComponent.IsLocked);

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Awake()
    {
        if (!door) door = GetComponent<DoorController>();
        if (!lockComponent) lockComponent = GetComponent<LockComponent>();

        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += OnInteract;

        saveStore = new PlayerPrefsSaveStore();
        LoadPersistentState();

        // Ensure hidden on scene load
        SetPromptVisible(false);
    }

    void OnEnable()
    {
        // Do not enable input until player is inside
        if (playerInside)
        {
            try { controls.Player.Enable(); } catch {}
        }
        else
        {
            SetPromptVisible(false);
        }
    }

    void OnDisable()
    {
        try { controls.Player.Disable(); } catch {}
        SetPromptVisible(false);
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
    }

    private void OnInteract(InputAction.CallbackContext _)
    {
        if (!playerInside || minigameActive) return;
        if (Time.unscaledTime - lastInteractTime < interactDebounce) return;
        lastInteractTime = Time.unscaledTime;

        if (door.IsOpen)
        {
            if (allowClose)
            {
                door.Close();
                UpdatePrompt();
            }
            return;
        }

        if (IsEffectivelyUnlocked)
        {
            door.Open();
            UpdatePrompt();
            return;
        }

        switch (mode)
        {
            case DoorAccessMode.Unlocked:
                door.Open();
                UpdatePrompt();
                break;

            case DoorAccessMode.Pickable:
                HandlePickable();
                break;

            case DoorAccessMode.KeyRequired:
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
            door.Open();
            UpdatePrompt();
        }
    }

    private void HandleKeyRequired()
    {
        if (IsEffectivelyUnlocked)
        {
            door.Open();
            UpdatePrompt();
            return;
        }

        bool hasKey = HasKey();
        if (hasKey)
        {
            if (consumeKey) InventoryManager.Instance?.RemoveItem(requiredKeyId);
            lockComponent?.ForceUnlock();
            SetPermanentlyUnlocked(true, alsoSwitchModeToUnlocked: true);
            door.Open();
            UpdatePrompt();
            return;
        }

        if (allowPickIfNoKey && lockComponent != null)
        {
            if (lockComponent.IsLocked)
            {
                StartLockpicking();
                return;
            }
            door.Open();
            UpdatePrompt();
            return;
        }

        // Requirement message will be shown only while inside trigger (guarded in UpdatePrompt)
        UpdatePrompt();
    }

    private void StartLockpicking()
    {
        if (!lockpickingPrefab)
        {
            Debug.LogWarning("Lockpicking prefab missing.");
            return;
        }

        minigameActive = true;
        SetPromptVisible(false);

        var ui = Instantiate(lockpickingPrefab);
        var follow = uiAnchor ? uiAnchor : transform;

        ui.Begin(lockComponent ? lockComponent.Difficulty : LockDifficulty.Medium, follow, success =>
        {
            minigameActive = false;
            if (success)
            {
                lockComponent?.ForceUnlock();
                SetPermanentlyUnlocked(true, alsoSwitchModeToUnlocked: true);
                door.Open();
            }
            else
            {
                NoiseSystem.Broadcast((Vector2)transform.position, failNoiseRadius, 1f);
            }

            if (playerInside) UpdatePrompt();
        });
    }

    private void UpdatePrompt()
    {
        if (!promptText) return;

        // Do not show any prompt if player is not inside or minigame is active
        if (!playerInside || minigameActive)
        {
            SetPromptVisible(false);
            return;
        }

        string keyName = SafeBindingDisplay();
        string keyLabel = string.IsNullOrEmpty(keyDisplayName) ? requiredKeyId : keyDisplayName;

        if (door.IsOpen)
        {
            if (allowClose)
            {
                promptText.text = $"Press {keyName} to close";
                SetPromptVisible(true);
            }
            else
            {
                SetPromptVisible(false);
            }
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
            case DoorAccessMode.Unlocked:
                promptText.text = $"Press {keyName} to open";
                SetPromptVisible(true);
                break;

            case DoorAccessMode.Pickable:
                promptText.text = $"Press {keyName} to pick lock";
                SetPromptVisible(true);
                break;

            case DoorAccessMode.KeyRequired:
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
                    SetPromptVisible(true);
                }
                else
                {
                    // Still show only while inside trigger
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

    private void LoadPersistentState()
    {
        if (string.IsNullOrEmpty(saveKey)) return;
        if (saveStore.TryGetBool(saveKey, out var val))
        {
            permanentlyUnlocked = val;
            if (permanentlyUnlocked && lockComponent) lockComponent.ForceUnlock();
        }
    }

    private void SetPermanentlyUnlocked(bool value, bool alsoSwitchModeToUnlocked)
    {
        permanentlyUnlocked = value;
        if (lockComponent && value) lockComponent.ForceUnlock();
        if (alsoSwitchModeToUnlocked) mode = DoorAccessMode.Unlocked;

        if (!string.IsNullOrEmpty(saveKey))
            saveStore.SetBool(saveKey, permanentlyUnlocked);
    }

    private string SafeBindingDisplay()
    {
        try { return controls.Player.Interact.GetBindingDisplayString(); }
        catch { return "Interact"; }
    }

    // IInteractable
    public bool CanInteract(object actor) => playerInside && !minigameActive;
    public void Interact(object actor) => OnInteract(default);
    public string GetPrompt(object actor) => promptText ? promptText.text : string.Empty;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, failNoiseRadius);
    }
#endif
}
