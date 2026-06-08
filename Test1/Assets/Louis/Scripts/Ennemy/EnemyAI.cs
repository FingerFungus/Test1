using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HitPoints))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [Range(1f, 30f)]
    public float sightRange  = 10f;
    public float meleeRange  = 1.5f;
    public float dashRange   = 5f;
    public LayerMask playerLayer;

    [Header("Sight Range Slider (optional)")]
    [Tooltip("Drag a UI Slider here to control sightRange at runtime.")]
    public Slider sightRangeSlider;

    [Header("Movement")]
    public float moveSpeed   = 4f;
    public float patrolRange = 4f;

    [Header("Melee")]
    public Transform      meleeHitbox;
    public SpriteRenderer meleeSprite;
    public float          meleeRadius           = 0.8f;
    public int[]          comboDamage           = { 5, 8, 10 };
    public float          comboWindow           = 0.6f;
    public float          meleeCooldown         = 1.2f;
    public float          hitboxVisibleDuration = 0.1f;
    public LayerMask      targetLayer;

    [Header("Dash")]
    public float      dashSpeed    = 18f;
    public float      dashDuration = 0.15f;
    public float      dashCooldown = 3f;
    public Collider2D bodyCollider;

    [Header("Jump")]
    public float jumpForce           = 12f;
    [Tooltip("Vertical threshold above which the enemy will jump toward the player.")]
    public float jumpHeightThreshold = 1.5f;
    [Tooltip("How often (seconds) the enemy may do a random jump while chasing.")]
    public float randomJumpInterval  = 3f;
    [Tooltip("Distance of the forward raycast that checks for ground obstacles.")]
    public float obstacleCheckDist   = 1.2f;

    [Header("On Death - Player Regen")]
    public int healthRegenOnKill = 20;
    [Tooltip("REQUIRED: assign your ground/platform layer — same one PlayerMovement uses.")]
    public LayerMask groundLayer;
    [Tooltip("Optional: a child Transform placed at the enemy's feet. If empty, position is estimated automatically.")]
    public Transform groundCheck;
    [Tooltip("Radius of the ground-check overlap circle.")]
    public float groundCheckRadius   = 0.15f;

    // ───────────────────────────────────────────────────────────
    private enum State { Patrol, Chase, Melee, Dash }
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
    private bool canMelee    = true;
    private bool isGrounded  = false;

    private int   comboStep   = 0;
    private float comboTimer  = 0f;
    private float hitboxTimer = 0f;

    private float nextRandomJumpTime = 0f;
    private bool  canJump            = true;

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

        HideMeleeHitbox();
        SetupSlider();
    }

    // ── Slider setup ────────────────────────────────────────────
    void SetupSlider()
    {
        if (sightRangeSlider == null) return;

        // Set min/max BEFORE value so Unity doesn't clamp to [0,1]
        sightRangeSlider.minValue = 1f;
        sightRangeSlider.maxValue = 30f;

        // Sync slider to current Inspector value without firing the callback
        sightRangeSlider.SetValueWithoutNotify(sightRange);

        // Clear any stale listeners (e.g. from a previous Play session)
        sightRangeSlider.onValueChanged.RemoveAllListeners();
        sightRangeSlider.onValueChanged.AddListener(OnSightSliderChanged);

        Debug.Log($"[EnemyAI] Slider wired. sightRange = {sightRange}");
    }

    void OnSightSliderChanged(float value)
    {
        sightRange = value;
        Debug.Log($"[EnemyAI] sightRange changed to {sightRange:F1}");
    }

    // ───────────────────────────────────────────────────────────
    //  Update
    // ───────────────────────────────────────────────────────────
    void Update()
    {
        CheckDeath();
        if (isDead || player == null || isDashing) return;

        CheckGrounded();
        TickTimers();
        UpdateState();
        RunState();
        FlipToward(player.position);
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

        // Grant player health regen
        if (player != null)
        {
            HitPoints playerHealth = player.GetComponent<HitPoints>();
            if (playerHealth != null)
            {
                playerHealth.currentHealth = Mathf.Min(playerHealth.maxHealth, playerHealth.currentHealth + healthRegenOnKill);
                playerHealth.onHealthChanged?.Invoke(playerHealth.currentHealth);
                Debug.Log($"Player regenerated {healthRegenOnKill} HP. Current: {playerHealth.currentHealth}/{playerHealth.maxHealth}");
            }
        }

        Destroy(gameObject, 0.1f);
    }

    // ───────────────────────────────────────────────────────────
    //  Ground check
    //  Uses a dedicated groundCheck transform if assigned, otherwise
    //  estimates the feet position from the collider bounds.
    // ───────────────────────────────────────────────────────────
    void CheckGrounded()
    {
        Vector2 origin;

        if (groundCheck != null)
        {
            origin = groundCheck.position;
        }
        else
        {
            // Estimate feet: bottom of the collider bounds
            Bounds b = bodyCollider != null
                ? bodyCollider.bounds
                : new Bounds(transform.position, Vector3.one * 0.5f);
            origin = new Vector2(b.center.x, b.min.y);
        }

        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded)
            Debug.Log($"[EnemyAI] {name} landed.");
    }

    // ───────────────────────────────────────────────────────────
    //  State machine
    // ───────────────────────────────────────────────────────────
    void UpdateState()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > sightRange)
            state = State.Patrol;
        else if (dist <= meleeRange && canMelee)
            state = State.Melee;
        else if (dist <= dashRange && dist > meleeRange && canDash)
            state = State.Dash;
        else
            state = State.Chase;
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

    // ───────────────────────────────────────────────────────────
    //  Patrol
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
    //  Chase + jumps
    // ───────────────────────────────────────────────────────────
    void DoChase()
    {
        float dir = (player.position.x > transform.position.x) ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        TryJump();
    }

    void TryJump()
    {
        if (!isGrounded || !canJump) return;

        // 1. Player is significantly above → jump toward them
        float heightDiff = player.position.y - transform.position.y;
        if (heightDiff > jumpHeightThreshold)
        {
            Jump("toward player");
            return;
        }

        // 2. Obstacle directly ahead → jump over it
        float forwardDir  = facingRight ? 1f : -1f;
        Vector2 rayOrigin = (Vector2)transform.position;
        RaycastHit2D hit  = Physics2D.Raycast(
            rayOrigin, Vector2.right * forwardDir, obstacleCheckDist, groundLayer);

        if (hit.collider != null)
        {
            Jump("over obstacle");
            return;
        }

        // 3. Periodic random jump while chasing
        if (Time.time >= nextRandomJumpTime)
        {
            nextRandomJumpTime = Time.time + randomJumpInterval;
            Jump("random");
        }
    }

    void Jump(string reason)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Debug.Log($"[EnemyAI] {name} jumped ({reason}). isGrounded was {isGrounded}");
        canJump = false;
        StartCoroutine(JumpCooldown());
    }

    IEnumerator JumpCooldown()
    {
        // Wait a short moment so physics has time to move the enemy off the ground
        yield return new WaitForSeconds(0.2f);
        // Then wait until they land again
        yield return new WaitUntil(() => isGrounded);
        canJump = true;
    }

    // ───────────────────────────────────────────────────────────
    //  Melee combo
    // ───────────────────────────────────────────────────────────
    void DoMelee()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (comboTimer <= 0f && comboStep > 0) comboStep = 0;

        PerformMeleeHit(comboStep);
        comboStep  = (comboStep + 1) % comboDamage.Length;
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
            var hp = col.GetComponent<HitPoints>();
            if (hp != null)
            {
                hp.TakeDamage(comboDamage[step]);
                Debug.Log($"Enemy hit {col.name} for {comboDamage[step]} dmg");
            }
        }
    }

    IEnumerator MeleeCooldownRoutine()
    {
        yield return new WaitForSeconds(meleeCooldown);
        canMelee = true;
    }

    // ───────────────────────────────────────────────────────────
    //  Dash
    // ───────────────────────────────────────────────────────────
    IEnumerator DoDash()
    {
        if (!canDash || isDashing) yield break;

        isDashing = true;
        canDash   = false;
        state     = State.Chase;

        float originalGravity = rb.gravityScale;
        rb.gravityScale       = 0f;
        bodyCollider.enabled  = false;

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

    // ───────────────────────────────────────────────────────────
    //  Hitbox helpers
    // ───────────────────────────────────────────────────────────
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

    // ───────────────────────────────────────────────────────────
    //  Flip
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashRange);

        // Obstacle raycast direction
        float fwd = Application.isPlaying ? (facingRight ? 1f : -1f) : 1f;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.right * fwd * obstacleCheckDist);

        // Ground check position
        if (groundCheck != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}