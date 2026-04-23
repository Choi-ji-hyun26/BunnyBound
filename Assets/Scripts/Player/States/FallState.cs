using UnityEngine;

public class FallState : PlayerState
{
    private float airAcceleration = 15f;

    public FallState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        stateMachine.Coordinator.Animator.SetBool("isJumping", true);
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

        if (stateMachine.IsGroundedCached)
        {
            stateMachine.currentJumpCount = 0;

            if (Mathf.Abs(h) > 0.1f)
                stateMachine.ChangeState(stateMachine.WalkState);
            else
                stateMachine.ChangeState(stateMachine.IdleState);

            return;
        }

        if (Mathf.Abs(h) > 0.1f)
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
            airAcceleration * Time.fixedDeltaTime
        );

        rigid.velocity = new Vector2(newXVelocity, rigid.velocity.y);
    }

    public override void ExitState()
    {
        stateMachine.Coordinator.Animator.SetBool("isJumping", false);
    }
}