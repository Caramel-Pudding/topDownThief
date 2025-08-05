using UnityEngine;

[CreateAssetMenu(menuName = "AI/GuardConfig")]
public class GuardConfig : ScriptableObject
{
    public float detectionTime = 1.5f; // сколько секунд нужно держать игрока в зоне
    public float loseTime = 1.0f;      // за сколько секунд "остывает" обнаружение
    public float chaseSpeed = 2.2f;    // скорость погони
}