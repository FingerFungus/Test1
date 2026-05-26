using UnityEngine;
using UnityEngine.Events;

public class HitPoints : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Invincibility Frames")]
    public float iFrameDuration = 0.5f;
    private float iFrameTimer = 0f;

    [Header("Events")]
    public UnityEvent<int> onDamageTaken;   // passes damage amount
    public UnityEvent      onDeath;
    public UnityEvent<int> onHealthChanged; // passes new health value

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth);
    }

    void Update()
    {
        if (iFrameTimer > 0f)
            iFrameTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Called by EnemyAI.PerformMeleeHit() — reduces health and fires events.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (iFrameTimer > 0f) return; // still invincible
        if (currentHealth <= 0)       return; // already dead

        currentHealth = Mathf.Max(0, currentHealth - amount);

        onDamageTaken?.Invoke(amount);
        onHealthChanged?.Invoke(currentHealth);

        Debug.Log($"{name} took {amount} damage — {currentHealth}/{maxHealth} HP remaining.");

        if (currentHealth <= 0)
            onDeath?.Invoke();
        else
            iFrameTimer = iFrameDuration;
    }

    /// <summary>
    /// Optional: restore health (pickups, regeneration, etc.)
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth);
    }
}