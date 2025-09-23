using UnityEngine;

public struct NoiseEvent
{
    public Vector2 Origin;
    public float CurrentRadius;
    public float MaxRadius;
    public float Timestamp;
    public float Strength; // optional, 0..1 if you compute it
}
