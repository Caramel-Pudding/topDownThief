using UnityEngine;

public struct NoiseInbox
{
    public bool HasSignal;
    public Vector2 Point;
    public float Time;
    public float Strength;
    public void Clear() => HasSignal = false;
}
