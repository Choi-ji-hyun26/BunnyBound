using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;
using UnityEngine.Timeline;

public class Bee : EnemyBase
{
    public Transform player;

    [Header("Detection")]
    public float detectRange = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Limits")]
    public float maxChaseDistance = 8f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float dashSpeed = 8f;
    public float returnSpeed = 3f;

    [HideInInspector] public Vector2 spawnPosition;

    public BeePatrolState PatrolState {get; private set;}
    public BeeDashState DashState {get; private set;}
    public BeeReturnState ReturnState {get; private set;}

    protected override void Awake()
    {
        base.Awake();
        // spawnPosition은 Start()로 이동
        // Awake()에서 저장하면 자식 오브젝트 초기화 타이밍과 충돌 가능

        PatrolState = new BeePatrolState(this, stateMachine);
        DashState = new BeeDashState(this, stateMachine);
        ReturnState = new BeeReturnState(this, stateMachine);

        stateMachine.Initialize(new BeePatrolState(this, stateMachine));
    }

    protected override void Start()
    {
        base.Start();
        // 모든 자식 오브젝트 초기화 완료 후 위치 저장
        spawnPosition = transform.position;
    }

    protected override void Update()
    {
        base.Update();
        FlipByDirection(rigid.velocity.x);
    }

    public float DistanceToPlayer()
    {
        if(player == null) 
            return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    public bool IsPlayerDetected()
    {
        float distance = DistanceToPlayer();
        
        if (distance <= detectRange)
        {
            // 장애물 확인용 Raycast
            Vector2 dir = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, playerLayer | obstacleLayer);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
                return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        // 기본 감지 범위 : 하늘색
        Gizmos.color = IsPlayerDetected() ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // 최대 추격 범위 : 노란색, 돌아가야 할 기준선
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPosition, maxChaseDistance);

        // 플레이어 감지 시 추적 선 : 빨간색
        if (player != null && IsPlayerDetected())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
