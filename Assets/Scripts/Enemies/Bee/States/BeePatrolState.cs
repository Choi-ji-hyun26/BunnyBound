using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Scripting.APIUpdating;

public class BeePatrolState : IEnemyState
{
    private Bee bee;
    private EnemyStateMachine stateMachine;

    private float moveDistance = 2f;
    public BeePatrolState(Bee bee, EnemyStateMachine stateMachine)
    {
        this.bee = bee;
        this.stateMachine = stateMachine;
    }
    public void Enter()
    {
    }

    public void Update()
    {
        Move();

        if(bee.DistanceToPlayer() <= bee.detectRange)
        {
            stateMachine.ChangeState(bee.DashState);
        }
    }

    public void Exit()
    {
        
    }

    private void Move()
    {
        float offset = Mathf.Sin(Time.time * bee.moveSpeed) * moveDistance;
        float targetY = bee.spawnPosition.y + offset;

        float directionY = targetY - bee.transform.position.y;
        bee.Rigid.velocity = new Vector2(0f, directionY);
    }
}
