using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class DashAbility : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    public Collider2D playerCollider;

    private Rigidbody2D rb;
    private bool isDashing = false;

    // MUST be public so DashBlocker can access it
    public bool canDash = true;

    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.right;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerCollider == null)
            playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        moveInput = new Vector2(
            Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0,
            Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0
        );

        // Track last horizontal direction the player was moving
        if (moveInput.x != 0)
            facingDirection = new Vector2(moveInput.x, 0).normalized;

        if (Keyboard.current.qKey.wasPressedThisFrame && canDash && !isDashing)
            StartCoroutine(Dash(facingDirection));
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        canDash = false;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        playerCollider.enabled = false;

        rb.linearVelocity = direction * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        playerCollider.enabled = true;
        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
