using UnityEngine;

[CreateAssetMenu(menuName = "AI/Guard/Config")]
public class GuardConfig : ScriptableObject
{
    [Header("Movement")]
    public float patrolSpeed = 1.8f;
    public float chaseSpeed = 2.6f;

    [Header("Vision")]
    public float visionRadius = 6f;
    public float fov = 90f;
    public LayerMask obstaclesMask;

    [Header("Detection")]
    public float detectionTime = 1.2f;
    public float loseTime = 1.0f;
    public float graceAfterLost = 0.2f;

    [Header("Attack")]
    public float attackRange = 1.2f;        // distance to start attacking
    public float attackCooldown = 0.8f;     // seconds between attacks
    public int attackDamage = 1;            // damage per hit
}
