using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float sightRange    = 10f;
    public float meleeRange    = 1.5f;
    public float dashRange     = 5f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float moveSpeed     = 4f;
    public float patrolRange   = 4f;

    [Header("Melee")]
    public Transform meleeHitbox;
    public SpriteRenderer meleeSprite;
    public float meleeRadius   = 0.8f;
    public float[] comboDamage = { 5f, 8f, 10f };
    public float comboWindow   = 0.6f;
    public float meleeCooldown = 1.2f;
    public float hitboxVisibleDuration = 0.1f;
    public LayerMask targetLayer;

    [Header("Dash")]
    public float dashSpeed     = 18f;
    public float dashDuration  = 0.15f;
    public float dashCooldown  = 3f;
    public Collider2D bodyCollider;

    private enum State { Patrol, Chase, Melee, Dash }
    private State state = State.Patrol;

    private Rigidbody2D rb;
    private Transform player;
    private Vector3 patrolOrigin;
    private int patrolDir = 1;

    private bool facingRight = true;
    private bool isDashing   = false;
    private bool canDash     = true;
    private bool canMelee    = true;

    private int  comboStep    = 0;
    private float comboTimer  = 0f;
    private float hitboxTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolOrigin = transform.position;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        HideMeleeHitbox();
    }

    void Update()
    {
        if (player == null || isDashing) return;

        TickTimers();
        UpdateState();
        RunState();
        FlipToward(player.position);
    }

    void FixedUpdate()
    {
        // movement is handled in RunState via rb.linearVelocity
    }

    // ── State machine ──────────────────────────────────────────

    void UpdateState()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > sightRange)
        {
            state = State.Patrol;
        }
        else if (dist <= meleeRange && canMelee)
        {
            state = State.Melee;
        }
        else if (dist <= dashRange && dist > meleeRange && canDash)
        {
            state = State.Dash;
        }
        else
        {
            state = State.Chase;
        }
    }

    void RunState()
    {
        switch (state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase:  DoChase();  break;
            case State.Melee:  DoMelee();  break;
            case State.Dash:   StartCoroutine(DoDash()); break;
        }
    }

    // ── Behaviours ─────────────────────────────────────────────

    void DoPatrol()
    {
        rb.linearVelocity = new Vector2(patrolDir * moveSpeed * 0.5f, rb.linearVelocity.y);

        float distFromOrigin = transform.position.x - patrolOrigin.x;
        if (Mathf.Abs(distFromOrigin) >= patrolRange)
        {
            patrolDir *= -1;
            Flip();
        }
    }

    void DoChase()
    {
        float dir = (player.position.x > transform.position.x) ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    void DoMelee()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (comboTimer <= 0f && comboStep > 0) comboStep = 0;

        PerformMeleeHit(comboStep);
        comboStep = (comboStep + 1) % comboDamage.Length;
        comboTimer = comboWindow;

        ShowMeleeHitbox();
        hitboxTimer = hitboxVisibleDuration;

        if (comboStep == 0)
        {
            canMelee = false;
            StartCoroutine(MeleeCooldownRoutine());
        }
    }

    void PerformMeleeHit(int step)
    {
        if (meleeHitbox == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleeHitbox.position, meleeRadius, targetLayer);

        foreach (var col in hits)
        {
            col.GetComponent<PlayerHealth>()?.TakeDamage(comboDamage[step]);
            Debug.Log($"Enemy hit {col.name} for {comboDamage[step]}");
        }
    }

    IEnumerator DoDash()
    {
        if (!canDash || isDashing) yield break;

        isDashing = true;
        canDash   = false;
        state     = State.Chase; // prevent re-triggering dash mid-dash

        float originalGravity  = rb.gravityScale;
        rb.gravityScale        = 0f;
        bodyCollider.enabled   = false;

        Vector2 dir = (player.position.x > transform.position.x)
            ? Vector2.right : Vector2.left;

        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity    = Vector2.zero;
        bodyCollider.enabled = true;
        rb.gravityScale      = originalGravity;
        isDashing            = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    IEnumerator MeleeCooldownRoutine()
    {
        yield return new WaitForSeconds(meleeCooldown);
        canMelee = true;
    }

    // ── Hitbox helpers ─────────────────────────────────────────

    void ShowMeleeHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.GetComponent<Collider2D>().enabled = true;
        if (meleeSprite  != null) meleeSprite.enabled = true;
    }

    void HideMeleeHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.GetComponent<Collider2D>().enabled = false;
        if (meleeSprite  != null) meleeSprite.enabled = false;
    }

    void TickTimers()
    {
        comboTimer = Mathf.Max(0f, comboTimer - Time.deltaTime);

        if (hitboxTimer > 0f)
        {
            hitboxTimer -= Time.deltaTime;
            if (hitboxTimer <= 0f) HideMeleeHitbox();
        }
    }

    // ── Flip ───────────────────────────────────────────────────

    void FlipToward(Vector3 target)
    {
        bool shouldFaceRight = target.x > transform.position.x;
        if (shouldFaceRight != facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }
}