using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private InputAction _moveAction, _jumpAction; 
    private PlayerController _playerController;
    

private void Awake()
{
    _moveAction = InputSystem.actions.FindAction("Move");
    _jumpAction = InputSystem.actions.FindAction("Jump");

    _jumpAction.performed += Jump; 

    _playerController = GetComponent<PlayerController>();
}

private void Jump(InputAction.CallbackContext context)
{
    _playerController.Jump();
}
    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        _playerController.Move(moveInput);
    }
}
