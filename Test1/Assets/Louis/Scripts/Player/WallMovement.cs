using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class WallMovement : MonoBehaviour
{
    [Header("Wall Detection")]
    [SerializeField] private Transform  wallCheck;
    [SerializeField] private LayerMask  wallLayer;
    public float wallCheckRadius = 0.2f;

    [Header("Wall Slide")]
    public float wallSlidingSpeed = 2f;

    [Header("Wall Jump")]
    public float wallJumpingTime      = 0.2f;   // how long after leaving wall you can still jump
    public float wallJumpingDuration  = 0.4f;   // how long the wall jump force is applied
    public Vector2 wallJumpingPower   = new Vector2(8f, 16f);

    // ── Private state ───────────────────────────────────────────
    private Rigidbody2D rb;

    private bool  isWallSliding;
    private bool  isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingCounter;
    private float wallJumpingDurationCounter;

    // Expose so PlayerMovement can block horizontal input during wall jump
    public bool IsWallJumping => isWallJumping;

    // ───────────────────────────────────────────────────────────
    //  Init
    // ───────────────────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // ───────────────────────────────────────────────────────────
    //  Update
    // ───────────────────────────────────────────────────────────
    void Update()
    {
        WallSlide();
        WallJump();
    }

    // ───────────────────────────────────────────────────────────
    //  Wall slide — mirrors the video's logic exactly
    // ───────────────────────────────────────────────────────────
    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && GetHorizontal() != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    // ───────────────────────────────────────────────────────────
    //  Wall jump — mirrors the video's timer-based approach
    // ───────────────────────────────────────────────────────────
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping         = false;
            wallJumpingDirection  = -transform.localScale.x;  // push away from wall
            wallJumpingCounter    = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (JumpPressed() && wallJumpingCounter > 0f)
        {
            isWallJumping             = true;
            rb.linearVelocity         = new Vector2(
                wallJumpingDirection * wallJumpingPower.x,
                wallJumpingPower.y);
            wallJumpingDurationCounter = wallJumpingDuration;
            wallJumpingCounter        = 0f;

            // Flip if needed
            if (transform.localScale.x != wallJumpingDirection)
            {
                Vector3 s  = transform.localScale;
                s.x       *= -1f;
                transform.localScale = s;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }

        if (isWallJumping)
        {
            wallJumpingDurationCounter -= Time.deltaTime;
            if (wallJumpingDurationCounter <= 0f)
                StopWallJumping();
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    // ───────────────────────────────────────────────────────────
    //  Helpers
    // ───────────────────────────────────────────────────────────
    private bool IsWalled()
    {
        return wallCheck != null &&
            Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    private bool IsGrounded()
    {
        // PlayerMovement handles the real ground check;
        // we approximate with a small downward overlap here
        return rb.linearVelocity.y == 0f && rb.linearVelocity.y >= -0.1f;
    }

    private float GetHorizontal()
    {
        float mx = 0f;
        var keyboard = Keyboard.current;
        var gamepad  = Gamepad.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  mx -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) mx += 1f;
        }
        if (gamepad != null)
            mx += gamepad.leftStick.x.ReadValue();

        return Mathf.Clamp(mx, -1f, 1f);
    }

    private bool JumpPressed()
    {
        var keyboard = Keyboard.current;
        var gamepad  = Gamepad.current;

        return (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) ||
               (gamepad  != null && gamepad.buttonSouth.wasPressedThisFrame);
    }

    // ───────────────────────────────────────────────────────────
    //  Gizmos
    // ───────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (wallCheck == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}