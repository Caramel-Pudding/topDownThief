using System;
using UnityEngine;

public class GuardDetector : MonoBehaviour
{
    public GuardPerception perception;
    public GuardConfig config;
    public Transform player;

    public float Progress { get; private set; } // 0..1
    public bool IsSpotted => Progress >= 1f;

    public event Action OnDetectStarted;     // 0 -> >0
    public event Action OnFullySpotted;      // -> 1
    public event Action OnLost;              // -> 0

    private float lostGraceTimer;

    void Reset() { perception = GetComponent<GuardPerception>(); }

    public void Tick()
    {
        if (!perception || !config || !player) return;

        bool visible = perception.TryGetVisibility(player, out float visFactor);

        float delta = 0f;
        if (visible)
        {
            delta = (visFactor > 0f ? visFactor : 1f) * (Time.deltaTime / Mathf.Max(0.01f, config.detectionTime));
            lostGraceTimer = config.graceAfterLost;
        }
        else
        {
            if (lostGraceTimer > 0f) lostGraceTimer -= Time.deltaTime;
            else delta = -Time.deltaTime / Mathf.Max(0.01f, config.loseTime);
        }

        float prev = Progress;
        Progress = Mathf.Clamp01(Progress + delta);

        if (prev <= 0f && Progress > 0f) OnDetectStarted?.Invoke();
        if (prev < 1f && Progress >= 1f) OnFullySpotted?.Invoke();
        if (prev > 0f && Progress <= 0f) OnLost?.Invoke();
    }
}
