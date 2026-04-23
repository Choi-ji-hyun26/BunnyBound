using UnityEngine;

public abstract class PlayerState
{
    // 상태 머신, 리지드바디 등 핵심 컴포넌트를 참조
    protected PlayerStateMachine stateMachine;
    //protected Rigidbody2D rigid;

    // 생성자 : 상태 머신 인스턴스를 받아 초기화 
    public PlayerState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        //this.rigid = stateMachine.Coordinator.Rigid;
    }

    // 상태 진입 시 1회 실행
    public abstract void EnterState();
    // Update 로직(입력, 애니메이션 전환등)
    public abstract void UpdateState();
    // FixedUpdate 로직(물리 연산)
    public abstract void FixedUpdateState();
    // 상태 이탈 시 1회 실행
    public abstract void ExitState();
    
}