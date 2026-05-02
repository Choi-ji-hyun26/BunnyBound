using UnityEngine;

public class PiranhaAttackState : IEnemyState
{
    private Piranha piranha;
    private EnemyStateMachine stateMachine;

    private float timer;

    public PiranhaAttackState(Piranha piranha, EnemyStateMachine stateMachine)
    {
        this.piranha      = piranha;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        timer = 0f;
        piranha.FacePlayer();
        piranha.Animator.SetBool("isAttacking", true);
        // IsAttacking 세팅은 애니메이션 이벤트 Enbox()/Debox()에서 처리
    }

    public void Update()
    {
        AnimatorStateInfo stateInfo = piranha.Animator.GetCurrentAnimatorStateInfo(0);

        // Attack 클립 재생 중이면 대기
        if (stateInfo.IsName("Attack") && stateInfo.normalizedTime < 1.0f)
        {
            timer = 0f;
            return;
        }

        // Attack 클립 종료 후 cooldown 대기
        timer += Time.deltaTime;
        if (timer >= piranha.cooldownTime)
            stateMachine.ChangeState(piranha.IdleState);
    }

    public void Exit()
    {
        piranha.Debox();
    }
}
