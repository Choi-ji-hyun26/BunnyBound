using System.Collections;
using UnityEngine;

public class Piranha : EnemyBase
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Timing")]
    public float cooldownTime = 1.0f;

    [Header("Attack Collider")]
    [SerializeField] protected CircleCollider2D attackCollider;
    protected float defaultColliderX;

    private Transform player;
    public PiranhaIdleState IdleState { get; private set; }
    public PiranhaAttackState AttackState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (attackCollider == null)
            Debug.LogError("[Piranha] attackCollider가 Inspector에 연결되지 않았습니다!");
        else
        {
            defaultColliderX = Mathf.Abs(attackCollider.offset.x);
            attackCollider.enabled = false;
        }

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

        if (distance > detectionRadius || direction.y < -0.1f) return false;

        if (obstacleLayer.value != 0)
        {
            RaycastHit2D obstacleHit = Physics2D.Raycast(
                transform.position, direction.normalized, distance, obstacleLayer
            );
            if (obstacleHit.collider != null) return false;
        }

        RaycastHit2D playerHit = Physics2D.Raycast(
            transform.position, direction.normalized, distance, playerLayer
        );

        return playerHit.collider != null && playerHit.collider.CompareTag("Player");
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

    // ───────────────────────────────────────────
    // Animation Event
    // 3~4프레임: 실제 공격 판정 구간
    // Enbox: 콜라이더 ON + IsAttacking = true  → 이때만 쉴드 가드 가능
    // Debox: 콜라이더 OFF + IsAttacking = false → 공격 판정 종료
    // ───────────────────────────────────────────
    public void Enbox()
    {
        attackCollider.enabled = true;
        SetAttacking(true);
    }

    public void Debox()
    {
        attackCollider.enabled = false;
        SetAttacking(false);
    }

    /// <summary>
    /// EnemyBase.IsAttacking setter — PiranhaAttackState 및 Animation Event에서 호출
    /// </summary>
    public void SetAttacking(bool value) => IsAttacking = value;

    // ───────────────────────────────────────────
    // 쉴드 차단 stun — 공격 애니메이션 freeze
    // ───────────────────────────────────────────
    public override void Stun(float duration)
    {
        if (!IsStunned)
            StartCoroutine(StunRoutine(duration));
    }

    protected override IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;
        Debox(); // 콜라이더 OFF + IsAttacking = false

        animator.speed = 0f;

        yield return new WaitForSeconds(duration);

        animator.speed = 1f;
        IsStunned = false;

        stateMachine.ChangeState(IdleState);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsPlayerDetected() ? Color.red : Color.cyan;

        int segments = 20;
        Vector3 lastPos = transform.position + new Vector3(-detectionRadius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * (180f / segments) * Mathf.Deg2Rad;
            Vector3 nextPos = transform.position + new Vector3(
                Mathf.Cos(angle) * -detectionRadius,
                Mathf.Sin(angle) * detectionRadius, 0);

            Gizmos.DrawLine(lastPos, nextPos);
            lastPos = nextPos;
        }
        Gizmos.DrawLine(lastPos, transform.position + new Vector3(detectionRadius, 0, 0));

        if (IsPlayerDetected())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
