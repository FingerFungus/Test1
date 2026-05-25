using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damageAmount = 10;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            HitPoints health = collision.gameObject.GetComponent<HitPoints>();
            if (health != null)
            {
                health.TakeDamage(damageAmount); // Call the player's health reduction
            }
        }
    }
}
