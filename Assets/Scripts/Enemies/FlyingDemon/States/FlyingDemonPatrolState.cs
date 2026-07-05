using UnityEngine;

/// <summary>
/// FlyingDemon 순찰 상태
/// - 스폰 위치 기준 좌우 이동 (FLYING 클립)
/// - 매 Update마다 Raycast로 플레이어 감지 → ChaseState 전환
/// </summary>
public class FlyingDemonPatrolState : IEnemyState
{
    private FlyingDemon demon;
    private EnemyStateMachine stateMachine;
    private float timer;

    public FlyingDemonPatrolState(FlyingDemon demon, EnemyStateMachine stateMachine)
    {
        this.demon = demon;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        timer = 0f;
        demon.Animator.SetBool("isFlying", true);
        demon.Animator.SetBool("isAttacking", false);
    }

    public void Update()
    {
        // 좌우 사인파 순찰
        timer += Time.deltaTime;
        float targetX = demon.spawnPosition.x + Mathf.Sin(timer * demon.PatrolSpeed) * demon.PatrolHeight;
        Vector2 newPos = new Vector2(targetX, demon.transform.position.y);
        demon.Rigid.MovePosition(newPos);

        // 이동 방향에 따라 스프라이트 플립
        float dirX = targetX - demon.transform.position.x;
        demon.FlipByDirection(dirX);

        // 플레이어 감지 시 Chase
        if (demon.IsPlayerDetected())
            stateMachine.ChangeState(demon.ChaseState);
    }

    public void Exit()
    {
        timer = 0f;
    }
}
