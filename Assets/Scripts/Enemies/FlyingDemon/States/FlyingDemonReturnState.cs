using UnityEngine;

/// <summary>
/// FlyingDemon 복귀 상태
/// - 스폰 위치로 귀환 (FLYING 클립)
/// - 복귀 중 플레이어 재감지 시 ChaseState 전환
/// - 스폰 위치 도달 시 PatrolState 전환
/// </summary>
public class FlyingDemonReturnState : IEnemyState
{
    private FlyingDemon demon;
    private EnemyStateMachine stateMachine;

    private const float arrivalThreshold = 0.2f;

    public FlyingDemonReturnState(FlyingDemon demon, EnemyStateMachine stateMachine)
    {
        this.demon = demon;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        demon.Animator.SetBool("isFlying", true);
        demon.Animator.SetBool("isAttacking", false);
    }

    public void Update()
    {
        // 복귀 중 플레이어 재감지 → Chase
        if (demon.IsPlayerDetected())
        {
            stateMachine.ChangeState(demon.ChaseState);
            return;
        }

        // 스폰 위치로 이동
        Vector2 dir = (demon.spawnPosition - (Vector2)demon.transform.position).normalized;
        demon.Rigid.velocity = dir * demon.ReturnSpeed;
        demon.FlipByDirection(dir.x);

        // 스폰 위치 도달 → Patrol
        float dist = Vector2.Distance(demon.transform.position, demon.spawnPosition);
        if (dist <= arrivalThreshold)
        {
            demon.Rigid.velocity = Vector2.zero;
            stateMachine.ChangeState(demon.PatrolState);
        }
    }

    public void Exit()
    {
        demon.Rigid.velocity = Vector2.zero;
    }
}
