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


int jumpCount = 0; 
bool isGrounded;
float mx;
float jumpCooldown;

private void Update()
{
    var keyboard = Keyboard.current;
    mx = 0;
    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) mx += 1;
    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) mx -= 1;
    isGrounded = Physics2D.OverlapCircle(GroundCheck.position, 0.1f, groundLayer);

    if (isGrounded)
    {
        jumpCount = 0; 
    }

    if (Keyboard.current.spaceKey.wasPressedThisFrame && (isGrounded || jumpCount < extraJumps) && Time.time > jumpCooldown)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        jumpCount++;
        jumpCooldown = Time.time + 0.5f; 
    }
}

private void FixedUpdate()
{
    rb.linearVelocity = new Vector2(mx * speed, rb.linearVelocity.y);
}
}