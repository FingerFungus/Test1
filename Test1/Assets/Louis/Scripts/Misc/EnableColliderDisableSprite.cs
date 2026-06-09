using UnityEngine;

public class EnableSpecificCollider : MonoBehaviour
{
    public Collider2D targetCollider; // assign in Inspector

    void Start()
    {
        // Enable the chosen collider
        if (targetCollider != null)
            targetCollider.enabled = true;

        // Disable SpriteRenderer
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;
    }
}
