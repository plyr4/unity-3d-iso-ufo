using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector3 verticalMove;
    public Vector3 horizontalMove;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool fire;
    public bool altFire;

    [Header("Movement Settings")]
    public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;
#endif
    public void OnHorizontalMove(InputValue value)
    {
        HorizontalMoveInput(value.Get<Vector2>());
    }
    
    public void OnVerticalMove(InputValue value)
    {
        VerticalMoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            LookInput(value.Get<Vector2>());
        }
    }

    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
    }

    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }

    public void OnFire(InputValue value)
    {
        FireInput(value.isPressed);
    }

    public void OnAltFire(InputValue value)
    {
        AltFireInput(value.isPressed);
    }

    public void HorizontalMoveInput(Vector2 newMoveDirection)
    {
        horizontalMove = newMoveDirection;
    }

    public void VerticalMoveInput(Vector2 newMoveDirection)
    {
        verticalMove = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    public void FireInput(bool newFireState)
    {
        fire = newFireState;
    }

    
    public void AltFireInput(bool newAltFireState)
    {
        altFire = newAltFireState;
    }

#if !UNITY_IOS || !UNITY_ANDROID

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

#endif

}
