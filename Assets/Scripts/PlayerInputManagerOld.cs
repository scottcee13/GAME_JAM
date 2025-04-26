using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManagerOld : MonoBehaviour
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
        {
            JumpWasPressed = true;
            //JumpWasReleased = false;
        }
        else if (context.canceled)
        {
            //JumpWasPressed = false;
            JumpWasReleased = true;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementDirection = context.ReadValue<Vector2>();
    }
}
