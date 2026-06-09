using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Melee")]
    [SerializeField] private Transform meleeHitbox;
    [SerializeField] private SpriteRenderer meleeSprite;
    [SerializeField] private Vector2 hitboxOffset = new Vector2(0.8f, 0f);
    [SerializeField] private float meleeRadius = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Combo Damage (base values)")]
    [SerializeField] private float[] comboDamage = { 10f, 16f, 22f, 40f };

    [Header("Damage Buffs")]
    [Tooltip("Global base multiplier applied to comboDamage values. 1 = no change, 1.25 = +25%")]
    [SerializeField] private float baseDamageMultiplier = 1f;

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

        ShowHitbox();
        hitboxTimer = hitboxVisibleDuration;
    }

    private void PerformMeleeHit(int step)
    {
        Vector2 origin = GetHitOrigin();

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, meleeRadius, enemyLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col == null) continue;

            float baseDmg = comboDamage[Mathf.Clamp(step, 0, comboDamage.Length - 1)];
            float finalDmg = baseDmg * baseDamageMultiplier;

            // Apply damage to enemy if they have an Enemy component
            // col.GetComponent<Enemy>()?.TakeDamage(finalDmg);
            Debug.Log($"Combo hit {step + 1} on {col.name} for {finalDmg} dmg (base {baseDmg} x {baseDamageMultiplier}x)");
        }
    }

    private Vector2 GetHitOrigin()
    {
        Vector2 basePos = meleeHitbox != null ? (Vector2)meleeHitbox.position : (Vector2)transform.position;
        Vector2 offset = new Vector2(hitboxOffset.x * facingDirection, hitboxOffset.y);
        return basePos + offset;
    }

    private void ShowHitbox()
    {
        if (meleeSprite != null) meleeSprite.enabled = true;
    }

    private void HideHitbox()
    {
        if (meleeSprite != null) meleeSprite.enabled = false;
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
        Vector2 origin = GetHitOrigin();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, meleeRadius);
    }

    // Optional runtime API to change base multiplier from other scripts
    public void SetBaseDamageMultiplier(float multiplier)
    {
        if (multiplier > 0f) baseDamageMultiplier = multiplier;
    }
}
