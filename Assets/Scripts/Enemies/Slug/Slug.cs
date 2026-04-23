using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slug : EnemyBase
{
    [SerializeField] private LayerMask groundLayer;
    public LayerMask GroundLayer => groundLayer; 
    
    protected override void Awake()
    {
        base.Awake();

        stateMachine.Initialize(new SlugPatrolState(this, stateMachine));
    }

    private void OnDrawGizmos()
    {
        if (BoxCollider == null) return;

        // 현재 방향에 따른 탐지 시작점 계산
        float detectionOffset = BoxCollider.size.x * 0.5f;
        float direction = (Rigid.velocity.x != 0) ? Mathf.Sign(Rigid.velocity.x) : 0;

        Vector2 frontVector = new Vector2(
            transform.position.x + (direction * detectionOffset), 
            transform.position.y
        );

        // 바닥 감지 레이 시각화
        Gizmos.color = Color.green;
        Vector3 downDist = Vector3.down * 1.1f;
        Gizmos.DrawRay(frontVector, downDist);

        Gizmos.DrawSphere(frontVector + (Vector2)downDist, 0.05f);
    }
}
