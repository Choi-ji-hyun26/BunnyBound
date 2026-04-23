using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piranha : EnemyBase
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f; // 원형 감지 범위
    [SerializeField] private LayerMask playerLayer;    // 플레이어 레이어 설정
    [SerializeField] private LayerMask obstacleLayer;  // 벽/지형 레이어 (선택 사항)

    [Header("Timing")]
    public float cooldownTime = 1.0f;

    protected CircleCollider2D attackCollider;
    protected float defaultColliderX;

    private Transform player;
    public PiranhaIdleState IdleState {get; private set;}
    public PiranhaAttackState AttackState {get; private set;}

    protected override void Awake() {
        base.Awake();

        attackCollider = GetComponent<CircleCollider2D>();
        defaultColliderX = Mathf.Abs(attackCollider.offset.x);
        attackCollider.enabled = false;

        IdleState = new PiranhaIdleState(this, stateMachine);
        AttackState = new PiranhaAttackState(this, stateMachine);
    }

    protected virtual void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        stateMachine.Initialize(new PiranhaIdleState(this, stateMachine));
    }

    public bool IsPlayerDetected()
    {
        if (player == null) return false;

        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // 거리 체크 
        if (distance <= detectionRadius && direction.y >= -0.1f) // -0.1f는 약간의 오차 허용
        {
            // Raycast로 장애물 확인
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, playerLayer | obstacleLayer);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    public void FacePlayer()
    {
        if (player == null) return;

        float dir = player.position.x - transform.position.x;
        FlipByDirection(dir);
        UpdateColliderPositionX();
    }

    private void UpdateColliderPositionX()
    {
        Vector2 offset = attackCollider.offset;
        offset.x = defaultColliderX * (spriteRenderer.flipX ? 1f : -1f);
        attackCollider.offset = offset;
    }

    // Animation Event
    public void Enbox() => attackCollider.enabled = true;
    public void Debox() => attackCollider.enabled = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = IsPlayerDetected() ? Color.red : Color.cyan;

        // 반원 그리기
        int segments = 20;
        Vector3 lastPos = transform.position + new Vector3(-detectionRadius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            // 0도에서 180도까지 계산
            float angle = i * (180f / segments) * Mathf.Deg2Rad;
            Vector3 nextPos = transform.position + new Vector3(Mathf.Cos(angle) * -detectionRadius, Mathf.Sin(angle) * detectionRadius, 0);
            
            Gizmos.DrawLine(lastPos, nextPos);
            lastPos = nextPos;
        }
        // 반원 밑변 닫기
        Gizmos.DrawLine(lastPos, transform.position + new Vector3(detectionRadius, 0, 0));

        // 플레이어 추적 선 
        if (IsPlayerDetected())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
