using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    public float jumpPower = 15f;
    public int extraJumps = 1;
    public float coyoteTime = 0.1f;
    public float groundCheckRadius = 0.5f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;

    private int jumpCount = 0;
    private bool isGrounded;
    private float mx;
    private float jumpCooldown;
    private float coyoteTimer;
    private bool facingRight = true; // track current facing direction

    private void Update()
    {
        var keyboard = Keyboard.current;
        var gamepad  = Gamepad.current;

        // --- Horizontal input ---
        mx = 0f;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  mx -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) mx += 1f;
        }
        if (gamepad != null)
            mx += gamepad.leftStick.x.ReadValue();

        mx = Mathf.Clamp(mx, -1f, 1f);

        // --- Flip ---
        if (mx > 0f && !facingRight) Flip();
        if (mx < 0f &&  facingRight) Flip();

        // --- Jump input ---
        bool jumpPressed = (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) ||
                           (gamepad  != null && gamepad.buttonSouth.wasPressedThisFrame);
        if (jumpPressed)
            Jump();

        CheckGrounded();
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
        bool canJump = (coyoteTimer > 0f || jumpCount <= extraJumps)
                       && Time.time > jumpCooldown;

        if (rb != null && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            jumpCount++;
            jumpCooldown = Time.time + 0.2f;
            coyoteTimer = 0f;
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck == null) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && rb != null && rb.linearVelocity.y <= 0f)
        {
            jumpCount  = 0;
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        }
    }
}