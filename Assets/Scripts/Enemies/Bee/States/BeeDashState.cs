using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeDashState : IEnemyState
{
    private Bee bee;
    private EnemyStateMachine stateMachine;
    private Vector2 dashDirection;

    public BeeDashState(Bee bee, EnemyStateMachine stateMachine)
    {
        this.bee = bee;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        if(bee.player != null)
        {
            dashDirection = ((Vector2)bee.player.position - (Vector2)bee.transform.position).normalized;
            bee.FlipByDirection(dashDirection.x);
        }
    }

    public void Update()
    {
        bee.Rigid.velocity = dashDirection * bee.dashSpeed;

        if(Vector2.Distance(bee.transform.position, bee.spawnPosition) > bee.maxChaseDistance){
            stateMachine.ChangeState(bee.ReturnState);
        }
    }

    public void Exit()
    {
        bee.Rigid.velocity = Vector2.zero;
    }
}
