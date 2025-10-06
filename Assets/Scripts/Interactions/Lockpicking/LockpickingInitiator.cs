
using UnityEngine;
using System;

public class LockpickingInitiator : MonoBehaviour
{
    [Header("Minigame UI")]
    [SerializeField] private LockpickingUI lockpickingPrefab;
    [SerializeField] private Transform uiAnchor;

    [Header("On Fail Noise")]
    [SerializeField] private float failNoiseRadius = 10f;
    [SerializeField] private float failExpandSpeed = 22f;
    [SerializeField] private float failLifeAfterReach = 0.15f;
    [SerializeField] private LayerMask failObstacleMask;

    private LockpickingUI activeMinigame;
    public bool IsMinigameActive => activeMinigame != null;

    public void StartLockpicking(LockDifficulty difficulty, Action<bool> onComplete)
    {
        if (!lockpickingPrefab)
        {
            Debug.LogWarning("Lockpicking prefab missing.");
            onComplete?.Invoke(false);
            return;
        }

        if (IsMinigameActive) return;

        activeMinigame = Instantiate(lockpickingPrefab);
        var follow = uiAnchor ? uiAnchor : transform;

        activeMinigame.Begin(difficulty, follow, success =>
        {
            if (!success)
            {
                EmitFailNoise();
            }
            activeMinigame = null;
            onComplete?.Invoke(success);
        });
    }

    public void CancelLockpicking()
    {
        if (!IsMinigameActive) return;
        
        activeMinigame.Cancel();
        activeMinigame = null;
    }

    private void EmitFailNoise()
    {
        NoiseSystem.Emit(
            (Vector2)transform.position,
            failNoiseRadius,
            failExpandSpeed,
            failLifeAfterReach,
            failObstacleMask
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
