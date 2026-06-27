using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBase : MonoBehaviour
{
    protected EnemyStateMachine stateMachine;

    [SerializeField] protected int maxHp = 1;
    protected int currentHp;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    [SerializeField] private bool defaultFacingLeft = true;

    // 이동형: 넉백 / 고정형: Hurt 트리거만 (스턴 없음)
    [SerializeField] protected bool isMovingEnemy = true;
    public bool IsMovingEnemy => isMovingEnemy;

    public bool IsStunned { get; protected set; } = false;

    /// <summary>
    /// 공격 판정 중 여부
    /// - 순찰형(Slime, Bat): 항상 false
    /// - 패턴형(Piranha): Enbox()/Debox() 애니메이션 이벤트에서 세팅
    /// </summary>
    public bool IsAttacking { get; protected set; } = false;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 0.5f;
    [SerializeField] private UnityEvent onDeath;

    [Header("Hit Reaction Settings")]
    [SerializeField] private float knockbackSpeed    = 5f;
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("Damage Number Settings")]
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 2f, 0f);

    protected Rigidbody2D rigid;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;

    public Rigidbody2D Rigid => rigid;
    public Animator Animator => animator;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public BoxCollider2D BoxCollider => boxCollider;

    // Player Transform 씬 전역 캐싱 — Bat/Piranha 개별 FindGameObjectWithTag 제거
    // 씬 재로드 시 null로 초기화되므로 OnDestroy에서 정리 불필요
    private static Transform cachedPlayer;
    protected static Transform CachedPlayer
    {
        get
        {
            if (cachedPlayer == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    cachedPlayer = playerObj.transform;
            }
            return cachedPlayer;
        }
    }

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
    /// - 생존 시 doHurt 트리거 + 피격 반응 + 데미지 숫자 표시
    ///   이동형: 수평 넉백 (IsStunned로 행동 캔슬)
    ///   고정형: doHurt 트리거만 — Hurt 애니메이션이 피격 표현 담당
    /// </summary>
    public virtual void TakeDamage(int amount, Vector2 hitPosition)
    {
        currentHp -= amount;

        ShowDamageNumber(amount);

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        animator.SetTrigger("doHurt");
        ApplyHitReaction(hitPosition);
    }

    /// <summary>
    /// 데미지 숫자 표시 — DamageNumberPool에서 꺼내서 피격 위치 위에 표시
    /// </summary>
    private void ShowDamageNumber(int amount)
    {
        if (DamageNumberPool.Instance == null) return;
        Vector3 spawnPos = transform.position + damageNumberOffset;
        DamageNumberPool.Instance.Get(amount, spawnPos);
    }

    /// <summary>
    /// 피격 반응
    /// 이동형: 수평 넉백
    /// 고정형: 반응 없음 (doHurt 트리거는 TakeDamage에서 이미 세팅)
    /// </summary>
    private void ApplyHitReaction(Vector2 hitPosition)
    {
        if (isMovingEnemy)
        {
            float dirX = transform.position.x - hitPosition.x;
            Vector2 knockbackDir = new Vector2(Mathf.Sign(dirX), 0f);
            TakeKnockback(knockbackDir, knockbackSpeed);
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
        OnKnockbackEnd();
    }

    /// <summary>
    /// 넉백 종료 콜백 — 넉백이 필요한 적에서 override
    /// </summary>
    protected virtual void OnKnockbackEnd() { }

    /// <summary>
    /// 고정형 적 전용 — 쉴드 차단 시 스턴
    /// Piranha에서 override: Debox() + doStun 트리거
    /// 일반 피격에서는 호출하지 않음
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
    /// - onDeath UnityEvent: 인스펙터에서 사망 시 실행할 콜백 연결
    /// - 예: FlyingDemon 사망 시 힌트 상자 SetActive(true)
    /// </summary>
    protected virtual void Die()
    {
        IsStunned = true;

        if (boxCollider != null) boxCollider.enabled = false;
        if (rigid != null) rigid.simulated = false;

        animator.SetTrigger("doDeath");
        onDeath?.Invoke();

        Object.Destroy(gameObject, deathDelay);
    }
}
