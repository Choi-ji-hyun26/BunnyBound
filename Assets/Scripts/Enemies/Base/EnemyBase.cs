using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    protected EnemyStateMachine stateMachine;

    [SerializeField] protected int maxHp = 1;
    protected int currentHp;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    [SerializeField] private bool defaultFacingLeft = true;

    // 이동형/고정형 구분 (Shield 넉백/스턴 제어)
    [SerializeField] protected bool isMovingEnemy = true; // Slug, Bee: true / Piranha: false
    public bool IsMovingEnemy => isMovingEnemy;

    // 스턴 상태 (넉백 경직 포함)
    public bool IsStunned { get; protected set; } = false;

    /// <summary>
    /// 공격 판정 중 여부
    /// - 순찰형(Slug, Bee): 항상 false — 쉴드 반응은 IsMovingEnemy로만 판단
    /// - 패턴형(Piranha): AttackState Enter/Exit에서 true/false 세팅
    ///   → 공격 중일 때만 쉴드 stun 발동
    /// </summary>
    public bool IsAttacking { get; protected set; } = false;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackSpeed = 8f;      // 넉백 이동 속도
    [SerializeField] private float knockbackDuration = 0.3f; // 넉백 지속 시간 → 거리 = speed * duration

    protected Rigidbody2D rigid;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;

    public Rigidbody2D Rigid => rigid;
    public Animator Animator => animator;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public BoxCollider2D BoxCollider => boxCollider;

    protected virtual void Awake()
    {
        stateMachine = new EnemyStateMachine();
        currentHp = maxHp;

        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Start() { }

    protected virtual void Update()
    {
        // 스턴/경직 중에는 상태머신 업데이트 안 함 → Move()의 velocity 덮어쓰기 차단
        if (IsStunned) return;
        stateMachine.Update();
    }

    public void FlipByDirection(float directionX)
    {
        if (Mathf.Abs(directionX) < 0.01f) return;
        bool movingLeft = directionX < 0f;
        spriteRenderer.flipX = defaultFacingLeft ? !movingLeft : movingLeft;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHp -= amount;
        if (currentHp <= 0)
            Die();
    }

    /// <summary>
    /// 이동형 적 전용 — 쉴드 차단 시 넉백
    /// velocity 직접 세팅으로 GravityScale/drag에 무관하게 일정한 넉백 거리 보장
    /// 이동 거리 = knockbackSpeed * knockbackDuration
    /// - 지상 적 (GravityScale > 0): 수평 방향만 적용 (y는 중력에 맡김)
    /// - 공중 적 (GravityScale = 0): 전달받은 방향 그대로 적용
    /// </summary>
    public void TakeKnockback(Vector2 direction, float speed)
    {
        if (rigid == null) return;
        if (IsStunned) return;

        Vector2 knockbackVelocity = rigid.gravityScale == 0f
            ? direction.normalized * speed
            : new Vector2(direction.x, 0f).normalized * speed;

        StartCoroutine(KnockbackRoutine(knockbackVelocity));
    }

    private IEnumerator KnockbackRoutine(Vector2 knockbackVelocity)
    {
        IsStunned = true;

        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rigid.velocity = rigid.gravityScale == 0f
                ? knockbackVelocity
                : new Vector2(knockbackVelocity.x, rigid.velocity.y);

            timer += Time.deltaTime;
            yield return null;
        }

        rigid.velocity = rigid.gravityScale == 0f
            ? Vector2.zero
            : new Vector2(0f, rigid.velocity.y);

        IsStunned = false;
    }

    /// <summary>
    /// 고정형 적 전용 — 쉴드 차단 시 일시 정지
    /// 하위 클래스에서 override 가능 (애니메이션 freeze 등)
    /// </summary>
    public virtual void Stun(float duration)
    {
        if (!IsStunned)
            StartCoroutine(StunRoutine(duration));
    }

    protected virtual IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;

        if (rigid != null)
            rigid.velocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        IsStunned = false;
    }

    protected virtual void Die()
    {
        stateMachine.ChangeState(new EnemyDeathState(this));
    }
}
