using TopDownThief.Interfaces;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private string playerTag = "Player"; // Или переиспользуй общий tag

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);
        }
    }
}
