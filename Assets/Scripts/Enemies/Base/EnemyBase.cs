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

    // 이동형: 넉백 / 고정형: 스턴
    [SerializeField] protected bool isMovingEnemy = true;
    public bool IsMovingEnemy => isMovingEnemy;

    public bool IsStunned { get; protected set; } = false;

    /// <summary>
    /// 공격 판정 중 여부
    /// - 순찰형(Slug, Bee, Slime, Bat): 항상 false
    /// - 패턴형(Piranha): Enbox()/Debox() 애니메이션 이벤트에서 세팅
    /// </summary>
    public bool IsAttacking { get; protected set; } = false;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 0.5f;

    [Header("Hit Reaction Settings")]
    [SerializeField] private float knockbackSpeed    = 5f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float hitStunDuration   = 0.3f;

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

    /// <summary>
    /// 피격 처리
    /// - HP 감소 → 사망 시 Die()
    /// - 생존 시 doHurt 트리거 + 피격 반응 (이동형: 넉백 / 고정형: 스턴)
    /// </summary>
    public virtual void TakeDamage(int amount, Vector2 hitPosition)
    {
        currentHp -= amount;
        if (currentHp <= 0)
        {
            Die();
            return;
        }

        animator.SetTrigger("doHurt");
        ApplyHitReaction(hitPosition);
    }

    /// <summary>
    /// 피격 반응 — isMovingEnemy 기준으로 분기
    /// 이동형(Slug, Bat, Slime): 수평 넉백
    /// 고정형(Piranha): 스턴
    /// </summary>
    private void ApplyHitReaction(Vector2 hitPosition)
    {
        if (isMovingEnemy)
        {
            // x축만 사용 → 수평 넉백으로 통일
            float dirX = transform.position.x - hitPosition.x;
            Vector2 knockbackDir = new Vector2(Mathf.Sign(dirX), 0f);
            TakeKnockback(knockbackDir, knockbackSpeed);
        }
        else
        {
            Stun(hitStunDuration);
        }
    }

    /// <summary>
    /// 이동형 적 전용 — 넉백 (쉴드 차단 / 일반 피격 공용)
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
    /// 고정형 적 전용 — 스턴 (쉴드 차단 / 일반 피격 공용)
    /// Piranha에서 override해서 animator.speed 제어
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
    /// 플레이어 HitBox에 충돌했을 때 EnemyHitBox에서 호출
    /// 충돌 반응이 필요한 적(Bat 등)에서 override
    /// </summary>
    public virtual void OnHitPlayer() { }

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void Die()
    {
        IsStunned = true;

        if (boxCollider != null) boxCollider.enabled = false;
        if (rigid != null) rigid.simulated = false;

        animator.SetTrigger("doDeath");

        Object.Destroy(gameObject, deathDelay);
    }
}
