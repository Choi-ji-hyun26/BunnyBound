using UnityEngine;

public class FallState : PlayerState
{
    private float airAcceleration = 15f;

    public FallState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        // isJumping은 JumpState에서만 세팅
        // JumpState → FallState 경로에선 이미 true 상태로 유지됨
        // 사다리 등 다른 경로로 진입 시 isJumping: false인 채로 Fall 처리
        stateMachine.Coordinator.Animator.SetBool("isGrounded", false);
    }

    public override void UpdateState()
    {
        if (!stateMachine.CanMove)
            return;

        var input = stateMachine.Input;

        if (input.JumpPressed && stateMachine.currentJumpCount < stateMachine.maxJumpCount)
        {
            stateMachine.ChangeState(stateMachine.JumpState);
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

        if (stateMachine.IsGroundedCached)
        {
            stateMachine.currentJumpCount = 0;
            stateMachine.Coordinator.Animator.SetBool("isGrounded", true);

            float hInput = stateMachine.Input.MoveInput.x;
            if (Mathf.Abs(hInput) > 0.1f)
                stateMachine.ChangeState(stateMachine.WalkState);
            else
                stateMachine.ChangeState(stateMachine.IdleState);
        }
    }

    public override void ExitState()
    {
        stateMachine.Coordinator.Animator.SetBool("isJumping", false);
        stateMachine.Coordinator.Animator.SetBool("isGrounded", true);
    }
}
