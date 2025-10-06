
using UnityEngine;

public class DoorLockingSystem : MonoBehaviour
{
    public enum DoorAccessMode { Unlocked, Pickable, KeyRequired }

    [Header("Mode")]
    [SerializeField] private DoorAccessMode mode = DoorAccessMode.Unlocked;

    [Header("State")]
    [SerializeField] private bool permanentlyUnlocked;

    [Header("Key Settings")]
    [SerializeField] private string requiredKeyId;
    [SerializeField] private string keyDisplayName;
    [SerializeField] private bool consumeKey = false;
    [SerializeField] private bool allowPickIfNoKey = false;

    [Header("Dependencies")]
    [SerializeField] private LockComponent lockComponent;

    [Header("Persistence")]
    [SerializeField] private string saveKey = "";

    private ISaveStore saveStore;

    public DoorAccessMode Mode => mode;
    public string RequiredKeyId => requiredKeyId;
    public string KeyDisplayName => keyDisplayName;
    public bool ConsumeKey => consumeKey;
    public bool AllowPickIfNoKey => allowPickIfNoKey;
    public LockComponent LockComponent => lockComponent;

    public bool IsEffectivelyUnlocked => permanentlyUnlocked || mode == DoorAccessMode.Unlocked || (lockComponent != null && !lockComponent.IsLocked);

    void Awake()
    {
        if (!lockComponent) TryGetComponent(out lockComponent);
        saveStore = new PlayerPrefsSaveStore();
        LoadPersistentState();
    }

    public bool HasKey()
    {
        return !string.IsNullOrEmpty(requiredKeyId)
               && InventoryManager.Instance != null
               && InventoryManager.Instance.HasItem(requiredKeyId);
    }

    public void SetPermanentlyUnlocked(bool value, bool alsoSwitchModeToUnlocked)
    {
        permanentlyUnlocked = value;
        if (lockComponent && value) lockComponent.ForceUnlock();
        if (alsoSwitchModeToUnlocked) mode = DoorAccessMode.Unlocked;
        if (!string.IsNullOrEmpty(saveKey)) saveStore.SetBool(saveKey, permanentlyUnlocked);
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
}
