using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    public Vector2 MovementDirection;
    public bool JumpPressed;

    public UnityEvent TimeShiftEvent = new();

    private void Start()
    {
        //TimeShiftEvent.AddListener(LevelBase.Instance.OnTimeShift);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        JumpPressed = context.performed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementDirection = context.ReadValue<Vector2>();
    }

    public void OnTimeShift(InputAction.CallbackContext context)
    {
        if (context.started) TimeShiftEvent.Invoke();
    }
}
