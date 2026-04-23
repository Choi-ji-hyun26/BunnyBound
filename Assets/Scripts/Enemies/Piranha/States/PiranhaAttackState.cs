using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PiranhaAttackState : IEnemyState
{
    private Piranha piranha;
    private EnemyStateMachine stateMachine;

    private float timer;
    public PiranhaAttackState(Piranha piranha, EnemyStateMachine stateMachine)
    {
        this.piranha = piranha;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        timer = 0f;
        piranha.FacePlayer();
        piranha.Animator.Play("Piranha_Attack");
    }

    public void Update()
    {
        AnimatorStateInfo stateInfo = piranha.Animator.GetCurrentAnimatorStateInfo(0);

        // 애니메이션이 아직 재생 중이라면 타이머를 초기화하거나 대기
        if (stateInfo.IsName("Piranha_Attack") && stateInfo.normalizedTime < 1.0f)
        {
            timer = 0f;
            return;
        }
         // 애니메이션이 끝난 후 쿨타임 타이머 작동
        timer += Time.deltaTime;
        if (timer >= piranha.cooldownTime)
        {
            stateMachine.ChangeState(piranha.IdleState);
        }
    }

    public void Exit()
    {
        piranha.Debox();
    }
}
