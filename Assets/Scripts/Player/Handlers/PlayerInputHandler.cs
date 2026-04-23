using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 ClimbInput { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool TransformPressed { get; private set; } // 변신 키 입력

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx =>
        {
            MoveInput = ctx.ReadValue<Vector2>();
            ClimbInput = MoveInput; // 모바일 스틱 공유
        };

        inputActions.Player.Move.canceled += _ =>
        {
            MoveInput = Vector2.zero;
            ClimbInput = Vector2.zero;
        };

        inputActions.Player.Climb.performed += ctx =>
            ClimbInput = ctx.ReadValue<Vector2>();

        inputActions.Player.Climb.canceled += _ =>
            ClimbInput = Vector2.zero;

        inputActions.Player.Jump.started += _ => JumpPressed = true;
        inputActions.Player.Jump.performed += _ => JumpHeld = true;
        inputActions.Player.Jump.canceled += _ => JumpHeld = false;

        inputActions.Player.Interact.started += _ => InteractPressed = true;
        inputActions.Player.Transform.started += _ => TransformPressed = true;

    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void LateUpdate()
    {
        // 단발 입력 리셋
        JumpPressed = false;
        InteractPressed = false;
        TransformPressed = false;
    }
}
