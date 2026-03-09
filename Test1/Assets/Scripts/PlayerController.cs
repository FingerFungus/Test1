using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 5f;
    private Rigidbody2D _rb;
    private Vector2 _moveVector;
    private bool _jumpRequested = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _rb.LinearVelocityX = _moveVector.x * _moveSpeed;

        if (_jumpRequested)
        {
            _rigidbody.LinearVelocityY = _jumpForce;
            _jumpRequested = false;
        }
    }
    public void Move(Vector2 moveInput)
    {
        _moveVector = moveVector;
    } 
    public void Jump()
    {
        _jumpRequested = true;
    }
}
