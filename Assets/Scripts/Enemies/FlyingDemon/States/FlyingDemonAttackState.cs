using UnityEngine;

/// <summary>
/// FlyingDemon 공격 상태
/// - 제자리에서 ATTACK 클립 1회 재생
/// - 불덩이 발사는 Animation Event → FlyingDemon.ShootFireball() 호출
/// - 클립 1사이클 완료(normalizedTime >= 1f) → ChaseState 전환
/// - 감지 범위 이탈 시 → ReturnState 전환
///
/// [애니메이터 설정]
/// - ATTACK 클립 Loop Time: OFF
/// - 입 열리는 프레임에 Animation Event → ShootFireball 등록
/// </summary>
public class FlyingDemonAttackState : IEnemyState
{
    private FlyingDemon demon;
    private EnemyStateMachine stateMachine;

    public FlyingDemonAttackState(FlyingDemon demon, EnemyStateMachine stateMachine)
    {
        this.demon = demon;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        demon.Rigid.velocity = Vector2.zero;
        demon.Animator.SetBool("isAttacking", true);
        // isFlying은 끄지 않음 — Flying → Attack 전이를 타야 하므로
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

        // ATTACK 클립 1사이클 완료 → Chase 복귀
        var stateInfo = demon.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1f)
            stateMachine.ChangeState(demon.ChaseState);
    }

    public void Exit()
    {
        demon.Animator.SetBool("isAttacking", false);
    }
}
