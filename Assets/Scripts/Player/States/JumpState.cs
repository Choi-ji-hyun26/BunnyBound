using UnityEngine;

public class JumpState : PlayerState
{
    // 공중에서 수평 이동을 위한 가속도
    private float airAcceleration = 15f;
    public JumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    // 상태 진입 시 1회 실행
    public override void EnterState()
    {
        stateMachine.Coordinator.Animator.SetBool("isJumping", true);

        // 점프 힘 적용 로직 1번 실행
        float appliedForce = (stateMachine.currentJumpCount == 0)
                                ? stateMachine.firstJumpForce
                                : stateMachine.doubleJumpForce;

        // 2단 점프 시 수직 속도 초기화 (점프 높이 통일시키기 위함)
        if (stateMachine.currentJumpCount > 0)
            stateMachine.Coordinator.Rigid.velocity = new Vector2(stateMachine.Coordinator.Rigid.velocity.x, 0);

        // 점프 힘 적용
        stateMachine.Coordinator.Rigid.velocity = new Vector2(stateMachine.Coordinator.Rigid.velocity.x, appliedForce);

        // 점프 횟수 증가
        stateMachine.currentJumpCount++;
        SoundManager.Instance.PlaySound(SoundType.Jump);
    }
    // Update 로직(입력, 애니메이션 전환등)
    public override void UpdateState()
    {
        if (!stateMachine.CanMove)
        {
            return;
        }

        var rigid = stateMachine.Coordinator.Rigid;
        var input = stateMachine.Input;
        float h = input.MoveInput.x;
        float climbY = input.ClimbInput.y;
        
        // 공중 2단 점프 로직
        if (input.JumpPressed && stateMachine.currentJumpCount < stateMachine.maxJumpCount)
        {
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

    #if UNITY_STANDALONE || UNITY_EDITOR
        // 가변 점프 로직, PC 전용
        if(rigid.velocity.y > 0 && !input.JumpHeld){ // 상승 중일 때 버튼을 떼면
            rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y * 0.5f);
        }
    #endif

        // // Climb로 전환(사다리 접촉)
        // if (stateMachine.HasLadder() && climbY > 0.1f)
        // {
        //     stateMachine.ChangeState(stateMachine.ClimbState);
        //     return;
        // }

        // if (stateMachine.HasLadder() && climbY < -0.1f && stateMachine.IsGroundedCached)
        // {
        //     stateMachine.IgnoreCurrentOneWayPlatform(); // 추가
        //     stateMachine.ChangeState(stateMachine.ClimbState);
        //     return;
        // }

        // Fall로 전환
        if(rigid.velocity.y <= 0f) // 속도가 0 이하가 되면 FallState로 전환
        {
            stateMachine.ChangeState(stateMachine.FallState);
            return;
        }

        // 스프라이트 방향 전환
        if(h != 0)
        {
            stateMachine.Coordinator.SpriteRenderer.flipX = h < 0;
        }
    }
    // FixedUpdate 로직(물리 연산)
    public override void FixedUpdateState()
    {
        // 공중에서 좌우 이동 처리
        var rigid = stateMachine.Coordinator.Rigid;
        float h = stateMachine.Input.MoveInput.x;

        float targetVelocity = h * stateMachine.maxSpeed;

        float newXVelocity = Mathf.Lerp(
            rigid.velocity.x,
            targetVelocity,
            airAcceleration * Time.fixedDeltaTime
        );
        // 수직 속도는 물리 엔진에 맡기고 수평 속도만 조절
        rigid.velocity = new Vector2(newXVelocity, rigid.velocity.y); 
    }
    // 상태 이탈 시 1회 실행
    public override void ExitState()
    {
        // FallState에서 처리
    }
}