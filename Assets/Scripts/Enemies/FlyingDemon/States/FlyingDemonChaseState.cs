using UnityEngine;

/// <summary>
/// FlyingDemon 추격 상태
/// - 플레이어 방향으로 이동 (FLYING 클립)
/// - 공격 사거리 진입 시 AttackState 전환
/// - 플레이어 이탈 시 ReturnState 전환
/// </summary>
public class FlyingDemonChaseState : IEnemyState
{
    private FlyingDemon demon;
    private EnemyStateMachine stateMachine;

    public FlyingDemonChaseState(FlyingDemon demon, EnemyStateMachine stateMachine)
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
        if (demon.Player == null) return;

        // 감지 범위 이탈 시 Return
        if (!demon.IsPlayerDetected())
        {
            stateMachine.ChangeState(demon.ReturnState);
            return;
        }

        // 공격 사거리 진입 + 쿨다운 완료 시 Attack
        if (demon.IsInAttackRange() && demon.CanAttack)
        {
            stateMachine.ChangeState(demon.AttackState);
            return;
        }

        // 공격 사거리 안 + 쿨다운 중 → 제자리에서 플레이어 응시하며 대기
        if (demon.IsInAttackRange())
        {
            demon.Rigid.velocity = Vector2.zero;
            float faceDir = demon.Player.position.x - demon.transform.position.x;
            demon.FlipByDirection(faceDir);
            return;
        }

        // 공격 사거리 밖 → 플레이어 방향으로 이동
        Vector2 dir = ((Vector2)demon.Player.position - (Vector2)demon.transform.position).normalized;
        demon.Rigid.velocity = dir * demon.ChaseSpeed;
        demon.FlipByDirection(dir.x);
    }

    public void Exit()
    {
        demon.Rigid.velocity = Vector2.zero;
    }
}
