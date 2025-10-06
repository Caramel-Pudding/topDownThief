
using UnityEngine;
using System;

[RequireComponent(typeof(DoorLockingSystem))]
public class DoorMinigameInitiator : MonoBehaviour
{
    [Header("Minigame UI")]
    [SerializeField] private LockpickingUI lockpickingPrefab;
    [SerializeField] private Transform uiAnchor;

    [Header("On Fail Noise")]
    [SerializeField] private float failNoiseRadius = 30f;

    private DoorLockingSystem lockingSystem;

    public bool IsMinigameActive { get; private set; }

    void Awake()
    {
        lockingSystem = GetComponent<DoorLockingSystem>();
    }

    public void StartLockpicking(Action<bool> onComplete)
    {
        if (!lockpickingPrefab)
        {
            Debug.LogWarning("Lockpicking prefab missing.");
            onComplete?.Invoke(false);
            return;
        }

        if (IsMinigameActive) return;

        IsMinigameActive = true;

        var ui = Instantiate(lockpickingPrefab);
        var follow = uiAnchor ? uiAnchor : transform;
        var difficulty = lockingSystem.LockComponent ? lockingSystem.LockComponent.Difficulty : LockDifficulty.Medium;

        ui.Begin(difficulty, follow, success =>
        {
            if (success)
            {
                lockingSystem.SetPermanentlyUnlocked(true, alsoSwitchModeToUnlocked: true);
            }
            else
            {
                EmitFailNoise();
            }

            IsMinigameActive = false;
            onComplete?.Invoke(success);
        });
    }

    private void EmitFailNoise()
    {
        NoiseSystem.Emit(
            (Vector2)transform.position,
            failNoiseRadius,
            18f,
            0.12f,
            LayerMask.GetMask("Obstacles")
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.25f); // Yellow, semi-transparent
        Gizmos.DrawSphere(transform.position, failNoiseRadius);
    }
#endif
}
