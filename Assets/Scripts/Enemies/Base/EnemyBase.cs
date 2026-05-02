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
    /// - 순찰형(Slug, Bee, Slime): 항상 false
    /// - 패턴형(Piranha): Enbox()/Debox() 애니메이션 이벤트에서 세팅
    /// </summary>
    public bool IsAttacking { get; protected set; } = false;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 0.5f; // Death 애니메이션 길이에 맞게 Inspector에서 조정

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackSpeed = 8f;
    [SerializeField] private float knockbackDuration = 0.3f;

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
        else
            animator.SetTrigger("doHurt");
    }

    /// <summary>
    /// 이동형 적 전용 — 쉴드 차단 시 넉백
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

    /// <summary>
    /// 사망 처리 — 상태머신 우회, 직접 처리
    /// 1. 충돌/물리 비활성화
    /// 2. 상태머신 Update 중단 (IsStunned 활용)
    /// 3. doDeath 트리거 → 각 적의 Animator에서 Death 클립 재생
    /// 4. deathDelay 후 오브젝트 파괴
    /// </summary>
    protected virtual void Die()
    {
        IsStunned = true; // Update() 내 stateMachine.Update() 중단

        if (boxCollider != null) boxCollider.enabled = false;
        if (rigid != null) rigid.simulated = false;

        animator.SetTrigger("doDeath");

        Object.Destroy(gameObject, deathDelay);
    }
}
