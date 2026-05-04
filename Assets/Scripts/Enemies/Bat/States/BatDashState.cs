using UnityEngine;

/// <summary>
/// 박쥐 돌격 상태 — 플레이어 위치를 매 프레임 추적
/// - Attack 클립 재생
/// - 플레이어가 detectRange 밖으로 이탈 시 ReturnState 전환 (이탈 감지 방식)
/// </summary>
public class BatDashState : IEnemyState
{
    private Bat bat;
    private EnemyStateMachine stateMachine;

    public BatDashState(Bat bat, EnemyStateMachine stateMachine)
    {
        this.bat          = bat;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        bat.Animator.SetBool("isAttacking", true);
    }

    public void Update()
    {
        if (bat.Player == null)
        {
            stateMachine.ChangeState(bat.ReturnState);
            return;
        }

        // 매 프레임 플레이어 방향 재계산 → flip + 돌격
        Vector2 direction = ((Vector2)bat.Player.position - (Vector2)bat.transform.position).normalized;
        bat.FlipByDirection(direction.x);
        bat.Rigid.velocity = direction * bat.DashSpeed;

        // 플레이어가 감지 범위 밖으로 이탈 시 복귀
        if (!bat.IsPlayerDetected())
            stateMachine.ChangeState(bat.ReturnState);
    }

    public void Exit()
    {
        bat.Rigid.velocity = Vector2.zero;
    }
}
