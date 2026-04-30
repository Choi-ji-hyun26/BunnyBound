using System.Collections;
using System.Collections.Generic;
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
        // IsAttacking 세팅은 애니메이션 이벤트 Enbox()/Debox()에서 처리
        // Enter 시점은 아직 공격 준비 구간(1~2프레임)이므로 여기서 세팅하지 않음
    }

    public void Update()
    {
        AnimatorStateInfo stateInfo = piranha.Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Piranha_Attack") && stateInfo.normalizedTime < 1.0f)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= piranha.cooldownTime)
        {
            stateMachine.ChangeState(piranha.IdleState);
        }
    }

    public void Exit()
    {
        piranha.Debox(); // Debox 내부에서 IsAttacking = false도 처리
    }
}
