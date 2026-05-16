using UnityEngine;

public class JumpState : PlayerState
{
    private float airAcceleration = 15f;

    public JumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        stateMachine.Coordinator.Animator.SetBool("isJumping", true);

        float appliedForce = (stateMachine.currentJumpCount == 0)
            ? stateMachine.firstJumpForce
            : stateMachine.doubleJumpForce;

        if (stateMachine.currentJumpCount > 0)
            stateMachine.Coordinator.Rigid.velocity = new Vector2(stateMachine.Coordinator.Rigid.velocity.x, 0);

        stateMachine.Coordinator.Rigid.velocity = new Vector2(stateMachine.Coordinator.Rigid.velocity.x, appliedForce);

        stateMachine.currentJumpCount++;
        SoundManager.Instance.PlaySound(SoundType.Jump);
    }

    public override void UpdateState()
    {
        if (!stateMachine.CanMove)
            return;

        var rigid = stateMachine.Coordinator.Rigid;
        var input = stateMachine.Input;
        float h = input.MoveInput.x;

        if (input.JumpPressed && stateMachine.currentJumpCount < stateMachine.maxJumpCount)
        {
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

    #if UNITY_STANDALONE || UNITY_EDITOR
        if (rigid.velocity.y > 0 && !input.JumpHeld)
            rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y * 0.5f);
    #endif

        if (rigid.velocity.y <= 0f)
        {
            stateMachine.ChangeState(stateMachine.FallState);
            return;
        }

        if (h != 0)
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

    public override void ExitState() { }
}
