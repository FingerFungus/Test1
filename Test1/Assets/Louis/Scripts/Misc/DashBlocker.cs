using UnityEngine;

public class DashBlocker : MonoBehaviour
{
    private DashAbility dash;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dash = other.GetComponent<DashAbility>();
            if (dash != null)
                dash.canDash = false;   // Disable dash
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && dash != null)
        {
            dash.canDash = true;        // Re-enable dash
        }
    }
}
