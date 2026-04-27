using UnityEngine;

public class IdleState : PlayerState
{
    private float decelerationRate = 35f;

    public IdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        stateMachine.Coordinator.Animator.SetBool("isWalking", false);
    }

    public override void UpdateState()
    {
        if (!stateMachine.CanMove)
            return;

        var rigid = stateMachine.Coordinator.Rigid;
        var input = stateMachine.Input;

        float h = input.MoveInput.x;
        float climbY = input.ClimbInput.y;

        if (input.JumpPressed && stateMachine.IsGroundedCached)
        {
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

        if (stateMachine.HasLadder() && climbY > 0.1f)
        {
            stateMachine.ChangeState(stateMachine.ClimbState);
            return;
        }

        // 바닥에서 아래 입력 시 사다리 진입
        if (stateMachine.HasLadder() && climbY < -0.1f && stateMachine.IsGroundedCached)
        {
            // OneWayPlatform을 잠시 통과 허용
            stateMachine.StartLadderDrop();
            stateMachine.ChangeState(stateMachine.ClimbState);
            return;
        }

        if (Mathf.Abs(h) > 0.1f)
        {
            stateMachine.ChangeState(stateMachine.WalkState);
            return;
        }

        if (!stateMachine.IsGroundedCached && rigid.velocity.y <= 0f)
        {
            stateMachine.ChangeState(stateMachine.FallState);
            return;
        }
    }

    public override void FixedUpdateState()
    {
        var rigid = stateMachine.Coordinator.Rigid;

        float newXVelocity = Mathf.Lerp(
            rigid.velocity.x,
            0f,
            decelerationRate * Time.fixedDeltaTime
        );

        if (Mathf.Abs(newXVelocity) < 0.05f)
            newXVelocity = 0f;

        rigid.velocity = new Vector2(newXVelocity, rigid.velocity.y);
    }

    public override void ExitState() { }
}