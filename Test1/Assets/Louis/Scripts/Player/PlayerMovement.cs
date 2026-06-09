using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    public float jumpPower = 15f;
    public int extraJumps = 1;
    public float coyoteTime = 0.1f;
    public float groundCheckRadius = 0.05f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;

    private Collider2D bodyCollider;
    private float mx;
    private float coyoteTimer;
    private bool wasGrounded;
    private bool facingRight = true;
    private int jumpsRemaining;
    private float jumpCooldown;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        bodyCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        jumpsRemaining = 1 + extraJumps;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        var gamepad  = Gamepad.current;

        mx = 0f;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  mx -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) mx += 1f;
        }
        if (gamepad != null)
            mx += gamepad.leftStick.x.ReadValue();

        mx = Mathf.Clamp(mx, -1f, 1f);

        if (mx > 0f && !facingRight) Flip();
        if (mx < 0f &&  facingRight) Flip();

        CheckGrounded();

        bool jumpPressed = (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) ||
                           (gamepad  != null && gamepad.buttonSouth.wasPressedThisFrame);
        if (jumpPressed)
            Jump();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void FixedUpdate()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(mx * speed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        bool canJump = (jumpsRemaining > 0 || coyoteTimer > 0f)
                       && Time.time > jumpCooldown;

        if (rb != null && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);

            // Consume a jump — coyote counts as using one too
            jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);
            coyoteTimer    = 0f;
            jumpCooldown   = Time.time + 0.2f;
        }
    }

    private void CheckGrounded()
    {
        Vector2 origin = GetGroundCheckOrigin();

        // Use OverlapCircleAll and ignore the player's own collider and triggers
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, groundCheckRadius, groundLayer);
        bool grounded = false;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i];
            if (col == null) continue;
            if (col == bodyCollider) continue;
            if (col.isTrigger) continue;
            grounded = true;
            break;
        }

        // Also require downward or near-zero vertical velocity to avoid counting while rising
        grounded = grounded && rb.linearVelocity.y <= 0.01f;

        if (grounded && !wasGrounded)
        {
            // Landed this frame — restore all jumps
            jumpsRemaining = 1 + extraJumps;
            coyoteTimer    = coyoteTime;
        }
        else if (!grounded && wasGrounded)
        {
            // Walked off ledge — start coyote window, remove the ground jump
            jumpsRemaining = extraJumps;
            coyoteTimer    = coyoteTime;
        }
        else if (!grounded)
        {
            coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        }

        wasGrounded = grounded;
    }

    private Vector2 GetGroundCheckOrigin()
    {
        if (groundCheck != null)
            return groundCheck.position;

        if (bodyCollider != null)
        {
            Bounds b = bodyCollider.bounds;
            return new Vector2(b.center.x, b.min.y - 0.05f);
        }

        return (Vector2)transform.position + Vector2.down * 0.5f;
    }

    // Debug draw to help tune the ground check in the editor
    private void OnDrawGizmosSelected()
    {
        Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, groundCheckRadius);
    }
}
