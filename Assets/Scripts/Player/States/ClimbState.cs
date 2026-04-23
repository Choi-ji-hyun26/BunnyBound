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
        stateMachine.SnapToLadderCenter();
    }

    public override void UpdateState()
    {
        if (!stateMachine.CanMove)
            return;

        var input = stateMachine.Input;
        float climbY = input.ClimbInput.y;
        Debug.Log($"climbY: {climbY}");
        float h = input.MoveInput.x;

        // Climb 중에는 ladder 유지
        if (stateMachine.CurrentLadder == null)
        {
            ExitToGroundOrFall();
            return;
        }

        if (input.JumpPressed)
        {
            stateMachine.RestoreLadderTopPlatform();
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

        if (Mathf.Abs(h) > 0.1f && Mathf.Abs(climbY) < 0.1f)
        {
            stateMachine.RestoreLadderTopPlatform();

            if (stateMachine.IsGroundedCached)
                stateMachine.ChangeState(stateMachine.WalkState);
            else
                stateMachine.ChangeState(stateMachine.FallState);

            return;
        }

        Ladder ladder = stateMachine.CurrentLadder;
        float playerCenterY = stateMachine.Coordinator.BoxCollider.bounds.center.y;

        // 꼭대기 탈출
        if (climbY > 0.1f && playerCenterY >= ladder.TopY - 0.05f)
        {
            stateMachine.RestoreLadderTopPlatform();
            stateMachine.MoveToLadderTopMount();
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }

        // 바닥 탈출
        if (climbY < -0.1f && playerCenterY <= ladder.BottomY + 0.05f)
        {
            stateMachine.RestoreLadderTopPlatform();
            stateMachine.MoveToLadderBottomMount();
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }
    }

    public override void FixedUpdateState()
    {
        var rigid = stateMachine.Coordinator.Rigid;
        float climbY = stateMachine.Input.ClimbInput.y;

        stateMachine.SnapToLadderCenter();
        rigid.velocity = new Vector2(0f, climbY * stateMachine.ClimbSpeed);
    }

    public override void ExitState()
    {
        var rigid = stateMachine.Coordinator.Rigid;
        rigid.gravityScale = stateMachine.defaultGravity;
        stateMachine.Coordinator.Animator.SetBool("isClimbing", false);
    }

    private void ExitToGroundOrFall()
    {
        stateMachine.RestoreLadderTopPlatform();

        if (stateMachine.IsGroundedCached)
            stateMachine.ChangeState(stateMachine.IdleState);
        else
            stateMachine.ChangeState(stateMachine.FallState);
    }
}