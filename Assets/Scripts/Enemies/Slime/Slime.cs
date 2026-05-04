using UnityEngine;

/// <summary>
/// 슬라임 적
/// - 지상 순찰형: Patrol 애니메이션(Idle/Walk 통일)으로 이동
/// - ThinkRoutine이 방향 결정 시 jumpChance % 확률로 점프 삽입
/// - Hurt/Death 애니메이션 포함
/// </summary>
public class Slime : EnemyBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField, Range(0, 100)] private int jumpChance = 20; // Think 시 점프 발동 확률 (%)

    public float MoveSpeed  => moveSpeed;
    public float JumpForce  => jumpForce;
    public int   JumpChance => jumpChance;
    public LayerMask GroundLayer => groundLayer;

    public SlimePatrolState PatrolState { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PatrolState = new SlimePatrolState(this, stateMachine);
        stateMachine.Initialize(PatrolState);
    }

    /// <summary>
    /// BoxCollider 하단에서 BoxCast로 지면 감지
    /// origin을 콜라이더 하단으로 내려서 자기 자신 감지 오작동 방지
    /// </summary>
    public bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position
            + boxCollider.offset
            + Vector2.down * (boxCollider.size.y * 0.5f);

        Vector2 size     = new Vector2(boxCollider.size.x * 0.8f, 0.05f);
        float   distance = 0.1f;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, distance, groundLayer);
        return hit.collider != null;
    }

    /// <summary>
    /// 이동 방향 앞쪽 플랫폼 끝 감지
    /// </summary>
    public bool IsFacingEdge(int direction)
    {
        float   detectionOffset = boxCollider.size.x * 0.5f;
        Vector2 frontVector     = new Vector2(
            rigid.position.x + (direction * detectionOffset),
            rigid.position.y
        );

        RaycastHit2D rayHit = Physics2D.Raycast(frontVector, Vector2.down, 1.1f, groundLayer);
        return rayHit.collider == null;
    }

    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;

        Vector2 origin = (Vector2)transform.position
            + boxCollider.offset
            + Vector2.down * (boxCollider.size.y * 0.5f);

        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireCube(
            origin + Vector2.down * 0.1f,
            new Vector3(boxCollider.size.x * 0.8f, 0.05f, 0f)
        );
    }
}
