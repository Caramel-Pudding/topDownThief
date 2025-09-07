using UnityEngine;

public class GuardNoiseListener : MonoBehaviour, INoiseListener
{
    public void OnNoiseHeard(Vector2 position, float intensity)
    {
        // Hook into your AI: set target "investigate point", raise alertness, etc.
        Debug.Log($"Guard hears noise at {position} (intensity {intensity})");
        // Example: GetComponent<GuardAI>()?.Investigate(position);
    }
}
