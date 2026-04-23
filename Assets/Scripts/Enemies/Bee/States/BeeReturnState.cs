using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeReturnState : IEnemyState
{
    private Bee bee;
    private EnemyStateMachine stateMachine;

    public BeeReturnState(Bee bee, EnemyStateMachine stateMachine)
    {
        this.bee = bee;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {     
    }

    public void Update()
    {
        Vector2 direction = (bee.spawnPosition - (Vector2)bee.transform.position).normalized;
        bee.Rigid.velocity = direction * bee.returnSpeed;

        if(Vector2.Distance(bee.transform.position, bee.spawnPosition) < 0.1f)
        {
            bee.Rigid.velocity = Vector2.zero;
            stateMachine.ChangeState(bee.PatrolState);
        }
    }

    public void Exit()
    {
        bee.Rigid.velocity = Vector2.zero;
    }
}
