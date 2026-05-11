using UnityEngine;

public class HitPoints : MonoBehaviour
{

    public int maxHealth = 100;
    public int currentHealth;


    public HealthBar healthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        
    }
    // Update is called once per frame
    void TakeDamage(int damage)
    {
        currentHealth -= damage;

        healthBar.SetHealth(currentHealth);
    }
}
