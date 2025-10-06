
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(DoorController), typeof(DoorLockingSystem), typeof(DoorAudio))]
[RequireComponent(typeof(DoorPromptController), typeof(DoorMinigameInitiator))]
public class DoorInteractionHandler : MonoBehaviour, IInteractable
{
    [Header("Interaction")][SerializeField] private string playerTag = "Player";
    [SerializeField] private bool allowClose = true;
    [SerializeField] private float interactDebounce = 0.1f;

    // Component references
    private DoorController doorController;
    private DoorLockingSystem lockingSystem;
    private DoorAudio doorAudio;
    private DoorPromptController promptController;
    private DoorMinigameInitiator minigameInitiator;

    private InputSystem_Actions controls;
    private bool playerInside;
    private float lastInteractTime;

    void Awake()
    {
        // Get all required components
        doorController = GetComponent<DoorController>();
        lockingSystem = GetComponent<DoorLockingSystem>();
        doorAudio = GetComponent<DoorAudio>();
        promptController = GetComponent<DoorPromptController>();
        minigameInitiator = GetComponent<DoorMinigameInitiator>();

        // Setup input
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += OnInteract;

        // Ensure trigger collider
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnEnable()
    {
        if (playerInside) controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
        promptController.Clear();
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
        controls.Player.Enable();
        promptController.UpdatePrompt(doorController, lockingSystem, allowClose);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        controls.Player.Disable();
        promptController.Clear();
    }

    private void OnInteract(InputAction.CallbackContext _)
    {
        if (!CanInteract(null)) return;
        if (Time.unscaledTime - lastInteractTime < interactDebounce) return;
        lastInteractTime = Time.unscaledTime;

        Interact(null);
    }

    public void Interact(object actor)
    {
        // Close door if open
        if (doorController.IsOpen)
        {
            if (allowClose) doorController.Close();
            promptController.UpdatePrompt(doorController, lockingSystem, allowClose);
            return;
        }

        // Open if unlocked
        if (lockingSystem.IsEffectivelyUnlocked)
        {
            doorController.Open();
            promptController.UpdatePrompt(doorController, lockingSystem, allowClose);
            return;
        }

        // Handle locked doors
        switch (lockingSystem.Mode)
        {
            case DoorLockingSystem.DoorAccessMode.Pickable:
                HandlePickable();
                break;

            case DoorLockingSystem.DoorAccessMode.KeyRequired:
                HandleKeyRequired();
                break;
        }
    }

    private void HandlePickable()
    {
        minigameInitiator.StartLockpicking(success =>
        {
            if (success) doorController.Open();
            else doorAudio.PlayLockedSound();

            if (playerInside) promptController.UpdatePrompt(doorController, lockingSystem, allowClose);
        });

        // Hide prompt during minigame
        promptController.Clear();
    }

    private void HandleKeyRequired()
    {
        if (lockingSystem.HasKey())
        {
            if (lockingSystem.ConsumeKey) InventoryManager.Instance?.RemoveItem(lockingSystem.RequiredKeyId);
            lockingSystem.SetPermanentlyUnlocked(true, alsoSwitchModeToUnlocked: true);
            doorController.Open();
        }
        else if (lockingSystem.AllowPickIfNoKey && lockingSystem.LockComponent != null)
        {
            HandlePickable(); // Defer to lockpicking logic
            return;
        }
        else
        {
            doorAudio.PlayLockedSound();
        }

        if (playerInside) promptController.UpdatePrompt(doorController, lockingSystem, allowClose);
    }

    // IInteractable Implementation
    public bool CanInteract(object actor) => playerInside && !minigameInitiator.IsMinigameActive;
    public string GetPrompt(object actor) => promptController.isActiveAndEnabled ? promptController.GetPrompt(actor) : string.Empty; // Simplified, actual text is on controller
}
