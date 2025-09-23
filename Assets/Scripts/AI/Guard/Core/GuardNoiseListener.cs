using UnityEngine;

[RequireComponent(typeof(GuardBrain))]
public class GuardNoiseListener : MonoBehaviour, INoiseListener
{
    [Header("Routing")]
    [SerializeField] private StateAsset investigateState;   // optional
    [SerializeField] private bool autoSwitchToInvestigate = true;

    [Header("Filters")]
    [SerializeField] private float minInterval = 0.25f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask losObstacles;

    private GuardBrain brain;
    private float lastHeardAt;

    void Awake()
    {
        brain = GetComponent<GuardBrain>();
    }

    public void OnNoiseHeard(NoiseEvent e)
    {
        if (Time.time - lastHeardAt < minInterval) return;

        Vector2 self = brain.transform.position;
        Vector2 to = e.Origin - self;
        if (to.sqrMagnitude > maxDistance * maxDistance) return;

        // Optional simple occlusion test for sound
        var blocked = Physics2D.Raycast(self, to.normalized, to.magnitude, losObstacles);
        if (blocked.collider) return;

        lastHeardAt = Time.time;

        float strength = 1f - Mathf.Clamp01(to.magnitude / Mathf.Max(e.MaxRadius, 0.001f));
        brain.Ctx.WriteNoise(e.Origin, Time.time, strength);

        if (autoSwitchToInvestigate && investigateState != null)
            brain.Switch(investigateState);
    }
}
