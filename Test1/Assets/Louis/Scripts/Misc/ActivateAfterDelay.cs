using UnityEngine;

public class ActivateAfterDelay : MonoBehaviour
{
    [Header("Assign the collider you want to activate")]
    public Collider2D targetCollider;

    [Header("Seconds to wait before activation")]
    public float delay = 20f;

    private SpriteRenderer spriteRenderer;
    private bool hasActivated = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure both are OFF at start
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (targetCollider != null)
            targetCollider.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated && other.CompareTag("Player"))
        {
            hasActivated = true;
            StartCoroutine(ActivateAfterTime());
        }
    }

    private System.Collections.IEnumerator ActivateAfterTime()
    {
        yield return new WaitForSeconds(delay);

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (targetCollider != null)
            targetCollider.enabled = true;
    }
}
