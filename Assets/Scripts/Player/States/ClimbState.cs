using UnityEngine;

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

        if (input.JumpPressed)
        {
            stateMachine.SetOnLadder(false);
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

        if (!stateMachine.IsOnLadder) return;

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

        rigid.velocity = new Vector2(0f, climbY * stateMachine.ClimbSpeed);
    }

    public override void ExitState()
    {
        var rigid = stateMachine.Coordinator.Rigid;
        rigid.gravityScale = stateMachine.defaultGravity;
        stateMachine.Coordinator.Animator.SetBool("isClimbing", false);
    }
}
