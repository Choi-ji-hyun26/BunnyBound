using UnityEngine;

/// <summary>
/// Flying Demon 적 — 공중 원거리 중간 보스
/// - 스폰 위치 기준 좌우 순찰
/// - Raycast로 플레이어 감지 시 Chase → 공격 사거리 진입 시 Attack (불덩이 발사)
/// - 플레이어 이탈 시 Return
/// - HP 100, 몸통 데미지 2 (1/2 하트), 불덩이 데미지 1 (1/4 하트)
///
/// [Inspector 설정]
/// - detectRange: 감지 범위
/// - attackRange: 공격 사거리 (detectRange보다 짧게)
/// - fireballPrefab: FireBall 프리팹
/// - firePoint: 불덩이 발사 위치 Transform
/// - playerLayer / obstacleLayer: Raycast 레이어
/// </summary>
public class FlyingDemon : EnemyBase
{
    [Header("Detection")]
    [SerializeField] private float detectRange = 7f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float returnSpeed = 2.5f;
    [SerializeField] private float patrolHeight = 3f;

    [Header("Attack")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 1.5f;

    public float DetectRange     => detectRange;
    public float AttackRange     => attackRange;
    public float PatrolSpeed     => patrolSpeed;
    public float ChaseSpeed      => chaseSpeed;
    public float ReturnSpeed     => returnSpeed;
    public float PatrolHeight    => patrolHeight;
    public GameObject FireballPrefab => fireballPrefab;
    public Transform FirePoint   => firePoint;

    // 공격 쿼다운 — 마지막 공격 시각 기록
    private float lastAttackTime = -999f;

    /// <summary>
    /// 쿼다운이 지나 다음 공격이 가능한지 여부
    /// </summary>
    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown;

    /// <summary>
    /// 공격 시각 갱신 — AttackState 진입 시 호출
    /// </summary>
    public void MarkAttackTime() => lastAttackTime = Time.time;

    [HideInInspector] public Vector2 spawnPosition;

    public Transform Player => CachedPlayer;

    public FlyingDemonPatrolState PatrolState { get; private set; }
    public FlyingDemonChaseState  ChaseState  { get; private set; }
    public FlyingDemonAttackState AttackState { get; private set; }
    public FlyingDemonReturnState ReturnState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        PatrolState = new FlyingDemonPatrolState(this, stateMachine);
        ChaseState  = new FlyingDemonChaseState(this, stateMachine);
        AttackState = new FlyingDemonAttackState(this, stateMachine);
        ReturnState = new FlyingDemonReturnState(this, stateMachine);
    }

    protected override void Start()
    {
        base.Start();

        spawnPosition = transform.position;
        stateMachine.Initialize(PatrolState);
    }

    // ─────────────────────────────────────────
    // 불덩이 발사 — Animation Event에서 호출
    // ─────────────────────────────────────────
    public void ShootFireball()
    {
        if (FireballPrefab == null || FirePoint == null) return;
        if (Player == null) return;

        float dir = Player.position.x > transform.position.x ? 1f : -1f;

        // 발사 방향에 맞춰 FirePoint 위치 좌우 보정
        // (spriteRenderer.flipX는 자식 Transform에 영향을 주지 않으므로 수동 반전)
        Vector3 firePointLocal = FirePoint.localPosition;
        firePointLocal.x = Mathf.Abs(firePointLocal.x) * dir;
        FirePoint.localPosition = firePointLocal;

        GameObject obj = Instantiate(FireballPrefab, FirePoint.position, Quaternion.identity);
        FireBall fireball = obj.GetComponent<FireBall>();
        fireball?.Initialize(dir);
    }

    // ─────────────────────────────────────────
    // 플레이어 감지 — Raycast 시야 기반
    // ─────────────────────────────────────────
    public bool IsPlayerDetected()
    {
        if (Player == null) return false;

        float distance = DistanceToPlayer();
        if (distance > detectRange) return false;

        Vector2 dir = (Player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, dir, distance,
            playerLayer | obstacleLayer
        );

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    public bool IsInAttackRange()
    {
        return DistanceToPlayer() <= attackRange;
    }

    public float DistanceToPlayer()
    {
        if (Player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, Player.position);
    }

    // ─────────────────────────────────────────
    // 넉백 종료 — Chase 재개
    // ─────────────────────────────────────────
    protected override void OnKnockbackEnd()
    {
        if (IsPlayerDetected())
            stateMachine.ChangeState(ChaseState);
        else
            stateMachine.ChangeState(ReturnState);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
