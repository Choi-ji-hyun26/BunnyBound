using UnityEngine;

public class FallState : PlayerState
{
    private float airAcceleration = 15f;

    public FallState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        stateMachine.Coordinator.Animator.SetBool("isJumping", true);
        stateMachine.Coordinator.Animator.SetBool("isGrounded", false);
    }

    public override void UpdateState()
    {
        if (!stateMachine.CanMove)
            return;

        var input = stateMachine.Input;
        float h = input.MoveInput.x;
        float climbY = input.ClimbInput.y;

        if (input.JumpPressed && stateMachine.currentJumpCount < stateMachine.maxJumpCount)
        {
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

        if (stateMachine.HasLadder() && climbY > 0.1f)
        {
            stateMachine.ChangeState(stateMachine.ClimbState);
            return;
        }

        // 낙하 중 아래 방향키 → 사다리 진입
        if (stateMachine.HasLadder() && climbY < -0.1f)
        {
            stateMachine.ChangeState(stateMachine.ClimbState);
            return;
        }

        if (stateMachine.IsGroundedCached)
        {
            stateMachine.currentJumpCount = 0;
            stateMachine.Coordinator.Animator.SetBool("isGrounded", true);

            if (Mathf.Abs(h) > 0.1f)
                stateMachine.ChangeState(stateMachine.WalkState);
            else
                stateMachine.ChangeState(stateMachine.IdleState);

            return;
        }
    }

    public override void FixedUpdateState()
    {
        var rigid = stateMachine.Coordinator.Rigid;
        float h = stateMachine.Input.MoveInput.x;

        float targetVelocity = h * stateMachine.maxSpeed;

        float newXVelocity = Mathf.Lerp(
            rigid.velocity.x,
            targetVelocity,
            airAcceleration * Time.fixedDeltaTime
        );

        rigid.velocity = new Vector2(newXVelocity, rigid.velocity.y);
    }

    public override void ExitState()
    {
        stateMachine.Coordinator.Animator.SetBool("isJumping", false);
        stateMachine.Coordinator.Animator.SetBool("isGrounded", true);
    }
}