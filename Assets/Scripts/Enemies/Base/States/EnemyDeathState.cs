using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeathState : IEnemyState
{
    private EnemyBase enemy;

    public EnemyDeathState(EnemyBase enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        // 1. 물리 및 충돌 비홠겅화
        enemy.BoxCollider.enabled = false;
        enemy.Rigid.simulated = false;
        // 2. 애니메이션 재생
        enemy.Animator.Play("Enemy_Death");
        // 4. 객체 삭제
        Object.Destroy(enemy.gameObject, 0.5f);
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
