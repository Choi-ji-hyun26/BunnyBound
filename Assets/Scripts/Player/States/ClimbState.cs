using UnityEngine;

/// <summary>
/// 사다리 오르내리기 상태
/// - LadderZone 진입 시 위아래 입력으로 진입
/// - gravity = 0, velocity.y = climbInput * climbSpeed
/// - 꼭대기/바닥 탈출은 LadderTop/LadderBottom 스크립트가 처리
/// - 탈출 조건: 점프, 좌우 이동(바닥), LadderZone 이탈
/// </summary>
public class ClimbState : PlayerState
{
    public ClimbState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        var rigid = stateMachine.Coordinator.Rigid;
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0f;

        stateMachine.Coordinator.Animator.SetBool("isClimbing", true);
    }

    public override void UpdateState()
    {
        if (!stateMachine.CanMove) return;

        var input = stateMachine.Input;
        float climbY = input.ClimbInput.y;
        float h = input.MoveInput.x;

        // 점프로 탈출
        if (input.JumpPressed)
        {
            stateMachine.SetOnLadder(false);
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

        // 사다리 Zone 이탈 시 자동 처리 (SetOnLadder false → FallState)
        if (!stateMachine.IsOnLadder) return;

        // 바닥에서 좌우 이동 시 탈출
        if (stateMachine.IsGroundedCached && Mathf.Abs(h) > 0.1f && Mathf.Abs(climbY) < 0.1f)
        {
            stateMachine.SetOnLadder(false);
            stateMachine.ChangeState(stateMachine.WalkState);
            return;
        }
    }

    public override void FixedUpdateState()
    {
        var rigid = stateMachine.Coordinator.Rigid;
        float climbY = stateMachine.Input.ClimbInput.y;

        // 위아래 이동 (X 고정)
        rigid.velocity = new Vector2(0f, climbY * stateMachine.ClimbSpeed);
    }

    public override void ExitState()
    {
        var rigid = stateMachine.Coordinator.Rigid;
        rigid.gravityScale = stateMachine.defaultGravity;
        stateMachine.Coordinator.Animator.SetBool("isClimbing", false);
    }
}
