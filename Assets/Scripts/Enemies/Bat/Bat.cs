using UnityEngine;

/// <summary>
/// 박쥐 적
/// - 평소: 스폰 위치 기준 상하 순찰 (Fly 클립)
/// - 플레이어 감지 시: 플레이어 위치 추적 돌격 (Attack 클립)
/// - 플레이어 충돌 시: 반대 방향으로 튕겨나감 (BounceState)
/// - 플레이어 이탈 시: 스폰 위치로 복귀 (Fly 클립)
/// </summary>
public class Bat : EnemyBase
{
    [Header("Detection")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed  = 2f;
    [SerializeField] private float dashSpeed    = 8f;
    [SerializeField] private float returnSpeed  = 3f;
    [SerializeField] private float patrolHeight = 2f;

    [Header("Bounce")]
    [SerializeField] private float bounceForce    = 6f;
    [SerializeField] private float bounceDuration = 0.6f;

    public float DetectRange    => detectRange;
    public float PatrolSpeed    => patrolSpeed;
    public float DashSpeed      => dashSpeed;
    public float ReturnSpeed    => returnSpeed;
    public float PatrolHeight   => patrolHeight;
    public float BounceForce    => bounceForce;
    public float BounceDuration => bounceDuration;

    [HideInInspector] public Vector2 spawnPosition;

    public Transform Player { get; private set; }

    public BatPatrolState PatrolState { get; private set; }
    public BatDashState   DashState   { get; private set; }
    public BatReturnState ReturnState { get; private set; }
    public BatBounceState BounceState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        PatrolState = new BatPatrolState(this, stateMachine);
        DashState   = new BatDashState(this, stateMachine);
        ReturnState = new BatReturnState(this, stateMachine);
        BounceState = new BatBounceState(this, stateMachine);
    }

    protected override void Start()
    {
        base.Start();

        spawnPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            Player = playerObj.transform;
        else
            Debug.LogWarning("[Bat] Player 태그를 가진 오브젝트를 찾을 수 없습니다.");

        stateMachine.Initialize(PatrolState);
    }

    // 피격 후 강제 돌진 관련 (ex. bat의 플레이어 감지 밖에서 플레이어가 원거리 공격을 한 경우)
    public bool IsForceDash { get; private set; }
    public Vector2 LastKnownPlayerPosition { get; private set; }

    // ─────────────────────────────────────────
    // 피격 후 상태 전환 — 넉백 종료 시 무조건 DashState
    // 피격 자체를 플레이어 인지 트리거로 사용
    // ─────────────────────────────────────────
    protected override void OnKnockbackEnd()
    {
        // BounceState 중이면 BounceRoutine이 상태 전환 담당 — 중복 전환 방지
        if (stateMachine.CurrentState == BounceState) return;

        IsForceDash = true;
        LastKnownPlayerPosition = Player.position;
        stateMachine.ChangeState(DashState);
    }

    public void ClearForceDash()
    {
        IsForceDash = false;
    }

    // ─────────────────────────────────────────
    // 플레이어 충돌 콜백 — EnemyHitBox에서 호출
    // DashState일 때만 BounceState로 전환
    // ─────────────────────────────────────────
    public override void OnHitPlayer()
    {
        if (stateMachine.CurrentState == DashState)
            stateMachine.ChangeState(BounceState);
    }

    // ─────────────────────────────────────────
    // 플레이어 감지
    // ─────────────────────────────────────────

    public float DistanceToPlayer()
    {
        if (Player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, Player.position);
    }

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

    private void OnDrawGizmos()
    {
        Gizmos.color = IsPlayerDetected() ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPosition, 0.3f);

        if (Player != null && IsPlayerDetected())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, Player.position);
        }
    }
}
