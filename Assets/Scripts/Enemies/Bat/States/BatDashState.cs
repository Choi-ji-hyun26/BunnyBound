using UnityEngine;

/// <summary>
/// 박쥐 돌격 상태 — 플레이어 위치를 매 프레임 추적
/// - Attack 클립 재생
/// - 플레이어가 detectRange 밖으로 이탈 시 ReturnState 전환 (이탈 감지 방식)
///
/// [IsForceDash 모드]
/// 피격으로 진입한 경우 — IsPlayerDetected() 체크 스킵
/// LastKnownPlayerPosition(피격 시점 위치)을 향해 이동
/// 해당 위치 도달 시 ClearForceDash() 후 ReturnState 전환
/// </summary>
public class BatDashState : IEnemyState
{
    private Bat bat;
    private EnemyStateMachine stateMachine;

    private const float arrivedThreshold = 0.3f;

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

        if (bat.IsForceDash)
        {
            // 피격으로 진입 — LastKnownPlayerPosition 향해 이동
            Vector2 forceDashDir = (bat.LastKnownPlayerPosition - (Vector2)bat.transform.position).normalized;
            bat.FlipByDirection(forceDashDir.x);
            bat.Rigid.velocity = forceDashDir * bat.DashSpeed;

            // 목표 위치 도달 시 ForceDash 해제 → ReturnState
            float dist = Vector2.Distance(bat.transform.position, bat.LastKnownPlayerPosition);
            if (dist <= arrivedThreshold)
            {
                bat.ClearForceDash();
                stateMachine.ChangeState(bat.ReturnState);
            }
            return;
        }

        // 일반 DashState — 매 프레임 플레이어 방향 재계산 → flip + 돌격
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
        bat.ClearForceDash();
    }
}
