using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Melee")]
    [SerializeField] private Transform meleeHitbox;
    [SerializeField] private SpriteRenderer meleeSprite;
    [SerializeField] private float meleeRadius = 0.8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float[] comboDamage = { 5f, 8f, 10f, 20f };

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 0.6f;
    [SerializeField] private float hitboxVisibleDuration = 0.1f;

    private int comboStep = 0;
    private float comboTimer = 0f;
    private float hitboxTimer = 0f;
    private int facingDirection = 1;

    private void Start()
    {
        HideHitbox();
    }

    private void Update()
    {
        TrackFacing();
        TickTimers();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryMeleeHit();
    }

    private void TryMeleeHit()
    {
        if (comboTimer <= 0f && comboStep > 0)
            comboStep = 0;

        PerformMeleeHit(comboStep);
        comboStep = (comboStep + 1) % comboDamage.Length;
        comboTimer = comboWindow;

        // Show hitbox briefly
        ShowHitbox();
        hitboxTimer = hitboxVisibleDuration;
    }

    private void PerformMeleeHit(int step)
    {
        if (meleeHitbox == null) return;

        Collider2D[] hit = Physics2D.OverlapCircleAll(
            meleeHitbox.position, meleeRadius, enemyLayer);

        foreach (var col in hit)
        {
            // col.GetComponent<Enemy>()?.TakeDamage(comboDamage[step]);
            Debug.Log($"Combo hit {step + 1} on {col.name} for {comboDamage[step]} dmg");
        }
    }

    private void ShowHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.GetComponent<Collider2D>().enabled = true;
        if (meleeSprite  != null) meleeSprite.enabled = true;
    }

    private void HideHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.GetComponent<Collider2D>().enabled = false;
        if (meleeSprite  != null) meleeSprite.enabled = false;
    }

    private void TickTimers()
    {
        comboTimer = Mathf.Max(0f, comboTimer - Time.deltaTime);

        if (hitboxTimer > 0f)
        {
            hitboxTimer -= Time.deltaTime;
            if (hitboxTimer <= 0f)
                HideHitbox();
        }
    }

    private void TrackFacing()
    {
        if (transform.localScale.x != 0)
            facingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    private void OnDrawGizmosSelected()
    {
        if (meleeHitbox == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleeHitbox.position, meleeRadius);
    }
}