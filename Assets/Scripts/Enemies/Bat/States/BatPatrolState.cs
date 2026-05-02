using UnityEngine;

/// <summary>
/// 박쥐 순찰 상태 — 스폰 위치 기준 상하 사인파 이동
/// - Fly 클립 재생
/// - 플레이어 감지 시 DashState 전환
/// </summary>
public class BatPatrolState : IEnemyState
{
    private Bat bat;
    private EnemyStateMachine stateMachine;

    public BatPatrolState(Bat bat, EnemyStateMachine stateMachine)
    {
        this.bat          = bat;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        bat.Animator.SetBool("isAttacking", false);

        // 순찰 중엔 x 이동이 없으므로 Enter 시점에 플레이어 방향으로 초기 flip
        if (bat.Player != null)
        {
            float dirToPlayer = bat.Player.position.x - bat.transform.position.x;
            bat.FlipByDirection(dirToPlayer);
        }
    }

    public void Update()
    {
        Patrol();

        if (bat.IsPlayerDetected())
            stateMachine.ChangeState(bat.DashState);
    }

    public void Exit()
    {
        bat.Rigid.velocity = Vector2.zero;
    }

    /// <summary>
    /// 스폰 위치 기준 사인파 상하 이동 — x velocity는 항상 0
    /// </summary>
    private void Patrol()
    {
        float offset  = Mathf.Sin(Time.time * bat.PatrolSpeed) * bat.PatrolHeight;
        float targetY = bat.spawnPosition.y + offset;

        float directionY = targetY - bat.transform.position.y;
        bat.Rigid.velocity = new Vector2(0f, directionY);
    }
}
