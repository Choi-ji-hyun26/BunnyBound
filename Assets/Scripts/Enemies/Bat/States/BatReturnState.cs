using UnityEngine;

/// <summary>
/// 박쥐 복귀 상태 — 스폰 위치로 귀환
/// - Fly 클립 재생 (isAttacking = false)
/// - 복귀 중 플레이어 재감지 시 DashState 재전환
/// - 스폰 위치 도착 시 PatrolState 전환
/// </summary>
public class BatReturnState : IEnemyState
{
    private Bat bat;
    private EnemyStateMachine stateMachine;

    public BatReturnState(Bat bat, EnemyStateMachine stateMachine)
    {
        this.bat          = bat;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        bat.Animator.SetBool("isAttacking", false);
    }

    public void Update()
    {
        // 복귀 중 플레이어 재감지 → 즉시 돌격 재개
        if (bat.IsPlayerDetected())
        {
            stateMachine.ChangeState(bat.DashState);
            return;
        }

        // 스폰 방향 계산 후 flip + 이동
        Vector2 direction = (bat.spawnPosition - (Vector2)bat.transform.position).normalized;
        bat.FlipByDirection(direction.x);
        bat.Rigid.velocity = direction * bat.ReturnSpeed;

        // 스폰 위치 도착
        if (Vector2.Distance(bat.transform.position, bat.spawnPosition) < 0.1f)
        {
            bat.Rigid.velocity = Vector2.zero;
            stateMachine.ChangeState(bat.PatrolState);
        }
    }

    public void Exit()
    {
        bat.Rigid.velocity = Vector2.zero;
    }
}
