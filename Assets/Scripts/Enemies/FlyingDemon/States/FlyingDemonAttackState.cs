using UnityEngine;

/// <summary>
/// FlyingDemon 공격 상태
/// - 제자리에서 불덩이 발사 (ATTACK 클립)
/// - attackCooldown마다 FireBall 생성
/// - 플레이어가 공격 사거리 이탈 시 ChaseState 전환
/// - 플레이어 감지 범위 이탈 시 ReturnState 전환
/// </summary>
public class FlyingDemonAttackState : IEnemyState
{
    private FlyingDemon demon;
    private EnemyStateMachine stateMachine;
    private float attackTimer;

    public FlyingDemonAttackState(FlyingDemon demon, EnemyStateMachine stateMachine)
    {
        this.demon = demon;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        demon.Animator.SetBool("isAttacking", true);
        demon.Rigid.velocity = Vector2.zero;
        attackTimer = demon.AttackCooldown; // 진입 즉시 발사
    }

    public void Update()
    {
        if (demon.Player == null) return;

        // 플레이어 방향 응시
        float dirX = demon.Player.position.x - demon.transform.position.x;
        demon.FlipByDirection(dirX);

        // 감지 범위 이탈 → Return
        if (!demon.IsPlayerDetected())
        {
            stateMachine.ChangeState(demon.ReturnState);
            return;
        }

        // 공격 사거리 이탈 → Chase
        if (!demon.IsInAttackRange())
        {
            stateMachine.ChangeState(demon.ChaseState);
            return;
        }

        // 쿨다운마다 불덩이 발사
        attackTimer += Time.deltaTime;
        if (attackTimer >= demon.AttackCooldown)
        {
            attackTimer = 0f;
            ShootFireball();
        }
    }

    public void Exit()
    {
        demon.Animator.SetBool("isAttacking", false);
        attackTimer = 0f;
    }

    private void ShootFireball()
    {
        if (demon.FireballPrefab == null || demon.FirePoint == null) return;

        float dir = demon.Player.position.x > demon.transform.position.x ? 1f : -1f;

        GameObject obj = Object.Instantiate(
            demon.FireballPrefab,
            demon.FirePoint.position,
            Quaternion.identity
        );

        FireBall fireball = obj.GetComponent<FireBall>();
        fireball?.Initialize(dir);
    }
}
