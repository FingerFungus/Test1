using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    public float jumpPower = 15f;
    public int extraJumps = 1;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform GroundCheck;

    private int jumpCount = 0;
    private bool isGrounded;
    private float mx;
    private float jumpCooldown;

    private void Update()
    {
        // Use new Input System instead of old Input.GetAxis
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;
        mx = 0f;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) mx -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) mx += 1f;
        }
        if (gamepad != null)
        {
            mx += gamepad.leftStick.x.ReadValue();
        }

        // Use new Input System for jump
        bool jumpPressed = (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) ||
                           (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
        if (jumpPressed)
        {
            Jump();
        }

        CheckGrounded();
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(mx * speed, rb.linearVelocity.y);
        }
    }



void Jump()
{
    // Fixed: Only allow extra jumps after the first (initial) jump
    if ((isGrounded || (jumpCount > 0 && jumpCount <= extraJumps)) && Time.time > jumpCooldown)
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        }
        jumpCount++;
        jumpCooldown = Time.time + 0.2f;  // Short cooldown to prevent spam
    }
}


void CheckGrounded()
{
    // Fixed: Simply check overlap; no incorrect cooldown logic
    isGrounded = Physics2D.OverlapCircle(GroundCheck.position, 0.5f, groundLayer);
    if (isGrounded && rb != null && rb.linearVelocity.y <= 0)
    {
        jumpCount = 0;
    }
}


}