using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class DoorInteractable : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DoorController door;
    [SerializeField] private LockComponent lockComponent;

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

        if (lockComponent != null && lockComponent.IsLocked)
        {
            StartLockpicking();
            return;
        }

        if (allowClose || !door.IsOpen)
        {
            door.Toggle();
            UpdatePrompt();
        }
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

        if (lockComponent != null && lockComponent.IsLocked)
        {
            promptText.text = $"Press {keyName} to pick lock";
            SetPromptVisible(!minigameActive);
            return;
        }

        if (door.IsOpen)
        {
            promptText.text = allowClose ? $"Press {keyName} to close" : string.Empty;
            SetPromptVisible(allowClose);
        }
        else
        {
            promptText.text = $"Press {keyName} to open";
            SetPromptVisible(true);
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
