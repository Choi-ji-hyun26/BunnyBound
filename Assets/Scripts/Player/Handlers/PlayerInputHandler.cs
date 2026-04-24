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

    // 검사 공격 입력
    public bool Attack1Pressed { get; private set; } // Q
    public bool Attack2Pressed { get; private set; } // W
    public bool Attack3Pressed { get; private set; } // E
    public bool Attack4Pressed { get; private set; } // R

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

        inputActions.Player.Attack1.started += _ => Attack1Pressed = true;
        inputActions.Player.Attack2.started += _ => Attack2Pressed = true;
        inputActions.Player.Attack3.started += _ => Attack3Pressed = true;
        inputActions.Player.Attack4.started += _ => Attack4Pressed = true;

    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void LateUpdate()
    {
        // 단발 입력 리셋
        JumpPressed = false;
        InteractPressed = false;
        TransformPressed = false;
        Attack1Pressed = false;
        Attack2Pressed = false;
        Attack3Pressed = false;
        Attack4Pressed = false;
    }
}
