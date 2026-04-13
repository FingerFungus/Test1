using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Melee")]
    [SerializeField] private Transform meleeHitbox;
    [SerializeField] private float meleeRadius = 0.8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float[] comboDamage = { 5f, 8f, 10f, 20f };

    [Header("Ranged")]
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float rangedCooldown = 0.5f;

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 0.6f;

    private int comboStep = 0;
    private float comboTimer = 0f;
    private float rangedCooldownTimer = 0f;
    private int facingDirection = 1;

    private void Update()
    {
        TrackFacing();
        TickTimers();
        HandleInput();
    }

    private void HandleInput()
    {
        var keyboard = Keyboard.current;
        var mouse    = Mouse.current;

        // Left click = melee
        bool meleePressed  = mouse    != null && mouse.leftButton.wasPressedThisFrame;
        // Z key = ranged
        bool rangedPressed = keyboard != null && keyboard.zKey.wasPressedThisFrame;

        if (meleePressed)  TryMeleeHit();
        if (rangedPressed) TryRangedShot();
    }

    private void TryMeleeHit()
    {
        if (comboTimer <= 0f && comboStep > 0)
            comboStep = 0;

        PerformMeleeHit(comboStep);
        comboStep++;
        comboTimer = comboWindow;

        if (comboStep >= comboDamage.Length)
            comboStep = 0;
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

    private void TryRangedShot()
    {
        if (rangedCooldownTimer > 0f || projectileSpawn == null || projectilePrefab == null)
            return;

        GameObject proj = Instantiate(
            projectilePrefab, projectileSpawn.position, Quaternion.identity);
        proj.GetComponent<Projectile>()?.Init(new Vector2(facingDirection, 0f));

        rangedCooldownTimer = rangedCooldown;
    }

    private void TickTimers()
    {
        comboTimer          = Mathf.Max(0f, comboTimer          - Time.deltaTime);
        rangedCooldownTimer = Mathf.Max(0f, rangedCooldownTimer - Time.deltaTime);
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