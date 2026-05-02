using System.Collections;
using UnityEngine;

/// <summary>
/// 박쥐 충돌 후 튕겨나가기 상태
/// - DashState에서 플레이어와 충돌 시 진입 (OnHitPlayer 콜백)
/// - 이동은 플레이어 반대 방향, 시선은 플레이어를 향해 유지
/// - bounceDuration 종료 후:
///     플레이어 감지됨 → DashState (즉시 재돌격)
///     플레이어 감지 안됨 → ReturnState (스폰 위치 복귀)
///
/// [전투 리듬]
/// DashState → BounceState → 재감지: DashState / 미감지: ReturnState → PatrolState
/// </summary>
public class BatBounceState : IEnemyState
{
    private Bat bat;
    private EnemyStateMachine stateMachine;

    public BatBounceState(Bat bat, EnemyStateMachine stateMachine)
    {
        this.bat          = bat;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        bat.Animator.SetBool("isAttacking", false);
        bat.StartCoroutine(BounceRoutine());
    }

    public void Update() { }

    public void Exit()
    {
        bat.Rigid.velocity = Vector2.zero;
    }

    private IEnumerator BounceRoutine()
    {
        Vector2 bounceDirection;
        if (bat.Player != null)
        {
            bounceDirection = ((Vector2)bat.transform.position - (Vector2)bat.Player.position).normalized;

            // 이동은 반대 방향이지만 시선은 플레이어를 응시
            float dirToPlayer = bat.Player.position.x - bat.transform.position.x;
            bat.FlipByDirection(dirToPlayer);
        }
        else
        {
            bounceDirection = (bat.spawnPosition - (Vector2)bat.transform.position).normalized;
        }

        bat.Rigid.velocity = bounceDirection * bat.BounceForce;

        // bounceDuration 동안 분리 — 플레이어 재정비 시간
        yield return new WaitForSeconds(bat.BounceDuration);

        // 플레이어 재감지 여부에 따라 분기
        if (bat.IsPlayerDetected())
            stateMachine.ChangeState(bat.DashState);    // 즉시 재돌격
        else
            stateMachine.ChangeState(bat.ReturnState);  // 스폰 위치 복귀
    }
}
