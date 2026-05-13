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
    [SerializeField] private float stunClipDuration = 0.3f;
    [SerializeField] private float stunHoldDuration = 0.5f;

    [Header("Attack Collider")]
    [SerializeField] protected CircleCollider2D attackCollider;
    protected float defaultColliderX;

    // EnemyBase.CachedPlayer 공유 — 개별 FindGameObjectWithTag 제거
    private Transform Player => CachedPlayer;

    private bool isDead = false;

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

        IdleState   = new PiranhaIdleState(this, stateMachine);
        AttackState = new PiranhaAttackState(this, stateMachine);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(IdleState);
    }

    public bool IsPlayerDetected()
    {
        if (Player == null) return false;

        Vector2 direction = Player.position - transform.position;
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
        if (Player == null) return;

        float dir = Player.position.x - transform.position.x;
        FlipByDirection(dir);
        UpdateColliderPositionX();
    }

    private void UpdateColliderPositionX()
    {
        Vector2 offset = attackCollider.offset;
        offset.x = defaultColliderX * (spriteRenderer.flipX ? 1f : -1f);
        attackCollider.offset = offset;
    }

    // ─────────────────────────────────────────
    // Animation Events
    // ─────────────────────────────────────────
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

    public void SetAttacking(bool value) => IsAttacking = value;

    // ─────────────────────────────────────────
    // 쉴드 차단 스턴
    // ─────────────────────────────────────────
    public override void Stun(float duration)
    {
        if (!IsStunned)
            StartCoroutine(StunRoutine(duration));
    }

    protected override IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;
        Debox();

        animator.SetTrigger("doStun");

        yield return new WaitForSeconds(stunClipDuration);

        animator.speed = 0f;
        yield return new WaitForSeconds(stunHoldDuration);
        animator.speed = 1f;

        IsStunned = false;

        if (!isDead)
            stateMachine.ChangeState(IdleState);
    }

    protected override void Die()
    {
        isDead = true;
        animator.speed = 1f;
        base.Die();
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

        if (Player != null && IsPlayerDetected())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, Player.position);
        }
    }
}
