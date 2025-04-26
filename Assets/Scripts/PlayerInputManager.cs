using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    private PlayerInput _playerInput;

    public Vector2 MovementDirection;
    public bool JumpPressed;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        JumpPressed = context.performed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementDirection = context.ReadValue<Vector2>();
    }
}
