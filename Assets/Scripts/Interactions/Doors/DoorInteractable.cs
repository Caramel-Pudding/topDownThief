using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class DoorInteractable : MonoBehaviour
{
    public enum DoorAccessMode { Unlocked, Pickable, KeyRequired }

    [Header("Refs")]
    [SerializeField] private DoorController door;
    [SerializeField] private LockComponent lockComponent; // used in Pickable/KeyRequired

    [Header("Mode")]
    [SerializeField] private DoorAccessMode mode = DoorAccessMode.Unlocked;

    [Header("Key Settings")]
    [SerializeField] private string requiredKeyId;
    [SerializeField] private string keyDisplayName;
    [SerializeField] private bool consumeKey = false;
    [SerializeField] private bool allowPickIfNoKey = false;

    [Header("Unlock Behavior")]

    [Header("UI")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Transform uiAnchor;
    [SerializeField] private LockpickingUI lockpickingPrefab;

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool allowClose = true;

    [Header("On Fail Noise")]
    [SerializeField] private float failNoiseRadius = 30f;

    private InputSystem_Actions controls;
    private bool playerInside;
    private bool minigameActive;

    // Internal state: once true, this door behaves as Unlocked forever (until you reset it manually)
    private bool permanentlyUnlocked;

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
    }

    void OnEnable() { if (playerInside) controls.Player.Enable(); }
    void OnDisable() { controls.Player.Disable(); }
    void OnDestroy()
    {
        controls.Player.Interact.performed -= OnInteract;
        controls.Dispose();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
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
        if (!playerInside || minigameActive) return;

        if (door.IsOpen)
        {
            if (allowClose)
            {
                door.Close();
                UpdatePrompt();
            }
            return;
        }

        // Short-circuit: if the door is effectively unlocked now, just open.
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
        // If already unlocked (e.g., previously opened), treat as unlocked.
        if (IsEffectivelyUnlocked)
        {
            door.Open();
            UpdatePrompt();
            return;
        }

        bool hasKey = !string.IsNullOrEmpty(requiredKeyId)
                      && InventoryManager.Instance != null
                      && InventoryManager.Instance.HasItem(requiredKeyId);

        if (hasKey)
        {
            if (consumeKey)
                InventoryManager.Instance.RemoveItem(requiredKeyId);

            lockComponent?.ForceUnlock();

            permanentlyUnlocked = true;
            // optional: switch mode to Unlocked for clarity
            // mode = DoorAccessMode.Unlocked;

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

        UpdatePrompt(); // show requirement
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
                permanentlyUnlocked = true;
                // optional: mode = DoorAccessMode.Unlocked;
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

        string keyName = controls.Player.Interact.GetBindingDisplayString();
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
                promptText.text = string.Empty;
                SetPromptVisible(false);
            }
            return;
        }

        // If effectively unlocked, always show open prompt
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
                if (lockComponent != null && lockComponent.IsLocked)
                {
                    promptText.text = $"Press {keyName} to pick lock";
                    SetPromptVisible(!minigameActive);
                }
                else
                {
                    promptText.text = $"Press {keyName} to open";
                    SetPromptVisible(true);
                }
                break;

            case DoorAccessMode.KeyRequired:
                bool hasKey = !string.IsNullOrEmpty(requiredKeyId)
                              && InventoryManager.Instance != null
                              && InventoryManager.Instance.HasItem(requiredKeyId);

                if (hasKey)
                {
                    promptText.text = consumeKey
                        ? $"Press {keyName} to use {keyLabel} and open"
                        : $"Press {keyName} to open (has {keyLabel})";
                    SetPromptVisible(true);
                }
                else
                {
                    if (allowPickIfNoKey && lockComponent != null)
                    {
                        promptText.text = $"Press {keyName} to pick lock (missing {keyLabel})";
                        SetPromptVisible(!minigameActive);
                    }
                    else
                    {
                        promptText.text = $"{keyLabel} required";
                        SetPromptVisible(true);
                    }
                }
                break;
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText) promptText.enabled = visible;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, failNoiseRadius);
    }
#endif
}
