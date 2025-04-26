using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    private PlayerInput _playerInput;

    public Vector2 MovementDirection;
    public bool JumpWasPressed;
    public bool JumpIsHeld;
    public bool JumpWasReleased;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            JumpWasPressed = true;
        else if (context.performed)
            JumpIsHeld = true;
        else if (context.canceled)
            JumpWasReleased = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementDirection = context.ReadValue<Vector2>();
    }
}
