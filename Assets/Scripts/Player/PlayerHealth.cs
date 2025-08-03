using TopDownThief.Interfaces;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 1;
    private int currentHealth;

    [SerializeField] private UnityEvent onDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        GlobalUI.Instance.ShowBlockerMessage("YOU DIED");
        onDeath?.Invoke();
        Time.timeScale = 0f; // Остановить игру
    }
}