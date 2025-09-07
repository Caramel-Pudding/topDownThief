using UnityEngine;

public interface INoiseListener
{
    void OnNoiseHeard(Vector2 position, float intensity);
}

public static class NoiseSystem
{
    public static void Broadcast(Vector2 position, float radius, float intensity = 1f, int maxColliders = 32, LayerMask layerMask = default)
    {
        var results = new Collider2D[maxColliders];
        int count = layerMask == default
            ? Physics2D.OverlapCircleNonAlloc(position, radius, results)
            : Physics2D.OverlapCircleNonAlloc(position, radius, results, layerMask);

        for (int i = 0; i < count; i++)
        {
            var col = results[i];
            if (!col) continue;
            var behaviours = col.GetComponents<MonoBehaviour>();
            for (int j = 0; j < behaviours.Length; j++)
            {
                if (behaviours[j] is INoiseListener listener)
                    listener.OnNoiseHeard(position, intensity);
            }
        }
    }
}
