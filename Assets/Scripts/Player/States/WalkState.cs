using UnityEngine;

public class WalkState : PlayerState
{
    private float accelerationRate = 25f;

    public WalkState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        stateMachine.Coordinator.Animator.SetBool("isWalking", true);
    }

    public override void UpdateState()
    {
        if (!stateMachine.CanMove)
            return;

        float h = stateMachine.Input.MoveInput.x;
        float climbY = stateMachine.Input.ClimbInput.y;

        if (stateMachine.Input.JumpPressed && stateMachine.IsGroundedCached)
        {
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

        if (stateMachine.HasLadder() && climbY > 0.1f)
        {
            stateMachine.ChangeState(stateMachine.ClimbState);
            return;
        }

        if (stateMachine.HasLadder() && climbY < -0.1f && stateMachine.IsGroundedCached)
        {
            stateMachine.IgnoreLadderTopPlatform();
            stateMachine.ChangeState(stateMachine.ClimbState);
            return;
        }

        if (Mathf.Abs(h) < 0.1f)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }

        if (!stateMachine.IsGroundedCached)
        {
            stateMachine.ChangeState(stateMachine.FallState);
            return;
        }

        stateMachine.Coordinator.SpriteRenderer.flipX = h < 0;
    }

    public override void FixedUpdateState()
    {
        var rigid = stateMachine.Coordinator.Rigid;
        float h = stateMachine.Input.MoveInput.x;

        float targetVelocity = h * stateMachine.maxSpeed;

        float newXVelocity = Mathf.Lerp(
            rigid.velocity.x,
            targetVelocity,
            accelerationRate * Time.fixedDeltaTime
        );

        rigid.velocity = new Vector2(newXVelocity, rigid.velocity.y);
    }

    public override void ExitState()
    {
        stateMachine.Coordinator.Animator.SetBool("isWalking", false);
    }
}