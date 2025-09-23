using UnityEngine;

public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance { get; private set; }

    [Header("Default Visuals")]
    [SerializeField] private Material ringMaterial;
    [SerializeField] private float ringWidth = 0.06f;
    [SerializeField, Range(8, 512)] private int raySamples = 128;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public static NoisePulse Emit(
        Vector2 origin,
        float maxRadius,
        float expandSpeed,
        float lifeAfterReach,
        LayerMask obstacleMask,
        float notifyInterval = 0.05f)
    {
        if (!Instance)
        {
            var go = new GameObject("[NoiseSystem]");
            Instance = go.AddComponent<NoiseSystem>();
        }
        return Instance.SpawnPulse(origin, maxRadius, expandSpeed, lifeAfterReach, obstacleMask, notifyInterval);
    }

    private NoisePulse SpawnPulse(
        Vector2 origin,
        float maxRadius,
        float expandSpeed,
        float lifeAfterReach,
        LayerMask obstacleMask,
        float notifyInterval)
    {
        var go = new GameObject("NoisePulse");
        go.transform.position = origin;
        var lr = go.AddComponent<LineRenderer>();
        var pulse = go.AddComponent<NoisePulse>();
        pulse.Init(new NoisePulse.Config
        {
            MaxRadius = maxRadius,
            ExpandSpeed = expandSpeed,
            LifeAfterReach = lifeAfterReach,
            ObstacleMask = obstacleMask,
            NotifyInterval = notifyInterval,
            RaySamples = raySamples,
            RingWidth = ringWidth,
            RingMaterial = ringMaterial
        });
        return pulse;
    }
}
