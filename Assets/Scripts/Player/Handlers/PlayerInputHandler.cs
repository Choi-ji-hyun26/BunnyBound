using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    // MoveInput: 조이스틱이 활성(터치 중)이면 조이스틱 값을, 아니면 Input System 값을 반환.
    // 모바일 터치와 키보드/게임패드 입력이 서로를 0으로 덮어쓰지 않도록 분리한다.
    public Vector2 MoveInput => hasJoystickInput ? joystickInput : keyboardMoveInput;

    // Input System(키보드/게임패드) Move 값
    private Vector2 keyboardMoveInput;

    // 커스텀 모바일 조이스틱 값 및 활성 여부
    private Vector2 joystickInput;
    private bool hasJoystickInput;

    /// <summary>
    /// 커스텀 조이스틱이 드래그 중 매 프레임 호출. 활성 상태로 전환하고 값을 밀어넣는다.
    /// </summary>
    public void SetJoystickInput(Vector2 value)
    {
        joystickInput = value;
        hasJoystickInput = true;
    }

    /// <summary>
    /// 조이스틱에서 손을 뗐을 때 호출. 오버라이드를 해제해 Input System 값으로 복귀한다.
    /// </summary>
    public void ClearJoystickInput()
    {
        joystickInput = Vector2.zero;
        hasJoystickInput = false;
    }

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
            keyboardMoveInput = ctx.ReadValue<Vector2>();

        inputActions.Player.Move.canceled += _ =>
            keyboardMoveInput = Vector2.zero;

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
