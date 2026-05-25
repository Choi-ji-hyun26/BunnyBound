using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool TransformPressed { get; private set; }

    // 검사 공격 입력
    public bool Attack1Pressed { get; private set; } // Q
    public bool Attack2Pressed { get; private set; } // W

    // 쉴드 입력
    // ShieldPressed : 단발 입력 (페링 방식 — LateUpdate에서 리셋)
    // ShieldHeld    : 홀드 입력 (다른 곳에서 참조 시 사용)
    public bool ShieldPressed { get; private set; }
    public bool ShieldHeld { get; private set; }

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx =>
            MoveInput = ctx.ReadValue<Vector2>();

        inputActions.Player.Move.canceled += _ =>
            MoveInput = Vector2.zero;

        inputActions.Player.Jump.started += _ => JumpPressed = true;
        inputActions.Player.Jump.performed += _ => JumpHeld = true;
        inputActions.Player.Jump.canceled += _ => JumpHeld = false;

        inputActions.Player.Interact.started += _ => InteractPressed = true;
        inputActions.Player.Transform.started += _ => TransformPressed = true;

        inputActions.Player.Attack1.started += _ => Attack1Pressed = true;
        inputActions.Player.Attack2.started += _ => Attack2Pressed = true;

        // ShieldPressed: started(키 누른 순간 1회) — 페링 단발 입력용
        // ShieldHeld: performed/canceled — 홀드 상태 유지용
        inputActions.Player.Shield.started += _ => ShieldPressed = true;
        inputActions.Player.Shield.performed += _ => ShieldHeld = true;
        inputActions.Player.Shield.canceled += _ => ShieldHeld = false;
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
        ShieldPressed = false; // 단발 — 매 프레임 리셋
        // ShieldHeld는 홀드 방식이라 리셋하지 않음
    }
}
