using TopDownThief.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 1;
    private int currentHealth;

    [SerializeField] private UnityEvent onDeath;

    private bool isDead;

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
        if (isDead) return;
        isDead = true;

        GlobalUI.Instance.ShowBlockerMessage("YOU DIED\nPress R to Restart");
        onDeath?.Invoke();
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (isDead && Keyboard.current.rKey.wasPressedThisFrame)
        {
            GameOverManager.RestartGame();
        }
    }
}