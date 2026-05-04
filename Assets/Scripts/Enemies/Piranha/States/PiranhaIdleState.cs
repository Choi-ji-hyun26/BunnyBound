using UnityEngine;

public class PiranhaIdleState : IEnemyState
{
    private Piranha piranha;
    private EnemyStateMachine stateMachine;

    public PiranhaIdleState(Piranha piranha, EnemyStateMachine stateMachine)
    {
        this.piranha      = piranha;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        piranha.Animator.SetBool("isAttacking", false);
    }

    public void Update()
    {
        if (piranha.IsPlayerDetected())
        {
            piranha.FacePlayer();
            stateMachine.ChangeState(piranha.AttackState);
        }
    }

    public void Exit() { }
}
