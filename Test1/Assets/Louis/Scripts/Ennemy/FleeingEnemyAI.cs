using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HitPoints))]
public class FleeingEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float fleeRange   = 8f;   // start fleeing when player enters this radius
    public float stopRange   = 14f;  // stop moving when player is beyond this radius
    public float dashRange   = 5f;   // panic-dash when player gets this close
    public LayerMask playerLayer;

    [Header("Movement")]
    public float moveSpeed   = 4f;
    public float patrolRange = 4f;

    [Header("Dash")]
    public float      dashSpeed    = 18f;
    public float      dashDuration = 0.15f;
    public float      dashCooldown = 3f;
    public Collider2D bodyCollider;

    private enum State { Patrol, Idle, Flee, Dash }
    private State state = State.Patrol;

    private Rigidbody2D rb;
    private HitPoints   hitPoints;
    private Transform   player;
    private Vector3     patrolOrigin;
    private int         patrolDir = 1;

    private bool facingRight = true;
    private bool isDashing   = false;
    private bool isDead      = false;
    private bool canDash     = true;

    // ───────────────────────────────────────────────────────────
    //  Init
    // ───────────────────────────────────────────────────────────
    void Start()
    {
        rb        = GetComponent<Rigidbody2D>();
        hitPoints = GetComponent<HitPoints>();

        patrolOrigin = transform.position;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();
    }

    // ───────────────────────────────────────────────────────────
    //  Update loop
    // ───────────────────────────────────────────────────────────
    void Update()
    {
        CheckDeath();
        if (isDead || player == null || isDashing) return;

        UpdateState();
        RunState();
        FlipToward(GetFleeDirection());
    }

    // ───────────────────────────────────────────────────────────
    //  Death
    // ───────────────────────────────────────────────────────────
    void CheckDeath()
    {
        if (!isDead && hitPoints.currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead            = true;
        rb.linearVelocity = Vector2.zero;
        Debug.Log($"{name} died.");
        Destroy(gameObject, 0.1f);
    }

    // ───────────────────────────────────────────────────────────
    //  State machine
    // ───────────────────────────────────────────────────────────
    void UpdateState()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= dashRange && canDash)
            state = State.Dash;
        else if (dist <= fleeRange)
            state = State.Flee;
        else if (dist <= stopRange)
            state = State.Idle;
        else
            state = State.Patrol;
    }

    void RunState()
    {
        switch (state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Idle:   DoIdle();   break;
            case State.Flee:   DoFlee();   break;
            case State.Dash:   StartCoroutine(DoDash()); break;
        }
    }

    // ───────────────────────────────────────────────────────────
    //  Patrol — wanders near spawn when player is far away
    // ───────────────────────────────────────────────────────────
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

    // ───────────────────────────────────────────────────────────
    //  Idle — player is in the "too far to matter" band; stand still
    // ───────────────────────────────────────────────────────────
    void DoIdle()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // ───────────────────────────────────────────────────────────
    //  Flee — run directly away from the player
    // ───────────────────────────────────────────────────────────
    void DoFlee()
    {
        float dir = (player.position.x > transform.position.x) ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    // ───────────────────────────────────────────────────────────
    //  Panic dash — burst away when player is very close
    // ───────────────────────────────────────────────────────────
    IEnumerator DoDash()
    {
        if (!canDash || isDashing) yield break;

        isDashing = true;
        canDash   = false;
        state     = State.Flee;

        float originalGravity = rb.gravityScale;
        rb.gravityScale       = 0f;
        bodyCollider.enabled  = false;

        // Dash AWAY from the player
        Vector2 dir = (player.position.x > transform.position.x)
            ? Vector2.left : Vector2.right;

        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity    = Vector2.zero;
        bodyCollider.enabled = true;
        rb.gravityScale      = originalGravity;
        isDashing            = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // ───────────────────────────────────────────────────────────
    //  Returns the direction this enemy should face:
    //  while fleeing/idling, face away from the player.
    //  While patrolling, face the patrol direction.
    // ───────────────────────────────────────────────────────────
    Vector3 GetFleeDirection()
    {
        if (state == State.Patrol)
        {
            // Return a point in the patrol direction so FlipToward works correctly
            return transform.position + Vector3.right * patrolDir;
        }

        // Face AWAY from the player — pass the opposite position
        Vector3 away = transform.position - (player.position - transform.position);
        return away;
    }

    // ───────────────────────────────────────────────────────────
    //  Flip helpers
    // ───────────────────────────────────────────────────────────
    void FlipToward(Vector3 target)
    {
        bool shouldFaceRight = target.x > transform.position.x;
        if (shouldFaceRight != facingRight) Flip();
    }

    void Flip()
    {
        facingRight          = !facingRight;
        Vector3 s            = transform.localScale;
        s.x                 *= -1f;
        transform.localScale = s;
    }

    // ───────────────────────────────────────────────────────────
    //  Gizmos
    // ───────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fleeRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopRange);
    }
}