using UnityEngine;

/// <summary>
/// W키 원거리 Slash 투사체
/// - 플레이어 방향으로 수평 이동 (maxDistance까지)
/// - IAttackHitBox 구현 → EnemyHurtBox.OnTriggerEnter2D에서 감지 → TakeDamage() 호출
/// - 관통: hitEnemies HashSet으로 적당 1번만 데미지
/// - maxDistance 도달 시 자동 소멸
/// - Layer: PlayerHitBox / IsTrigger: ON
///
/// [피격 흐름]
/// 투사체(PlayerHitBox) → EnemyHurtBox.OnTriggerEnter2D 감지
/// → IAttackHitBox.Damage 읽어서 enemy.TakeDamage() 호출
/// SlashProjectile.OnTriggerEnter2D — Breakable 태그 한정으로 BreakableBox 파괴 처리
/// 적 충돌(EnemyHurtBox)은 EnemyHurtBox.OnTriggerEnter2D에서 처리하므로 중복 없음
/// 관통 처리는 EnemyHurtBox가 Enter 기반이라 자연스럽게 관통됨
/// 같은 적의 여러 콜라이더에 중복 감지되는 것을 방지하기 위해
/// EnemyHurtBox에서 GetComponentInParent<EnemyBase>로 적 단위 중복 체크 필요
/// → 현재 EnemyHurtBox는 Enter 기반 단일 처리라 투사체 통과 시 1회만 발생
/// </summary>
public class SlashProjectile : MonoBehaviour, IAttackHitBox
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 16f;  // 이동 속도
    [SerializeField] private float maxDistance = 5f; // 최대 이동 거리

    private int damage = 15;
    public int Damage => damage;  // EnemyHurtBox에서 IAttackHitBox.Damage로 읽음

    private float direction = 1f;
    private Vector2 startPosition;
    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// PlayerSwordAttackHandler에서 생성 직후 호출
    /// </summary>
    public void Initialize(float direction, int damage)
    {
        this.direction = direction;
        this.damage = damage;
        startPosition = transform.position;

        // localScale.x로 방향 전환
        Vector3 scale = transform.localScale;
        scale.x = direction < 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Breakable")) return;

        IBreakable breakable = other.GetComponent<IBreakable>();
        breakable?.OnBreak();
    }

    private void FixedUpdate()
    {
        rigid.velocity = new Vector2(direction * moveSpeed, 0f);

        float distanceTraveled = Mathf.Abs(transform.position.x - startPosition.x);
        if (distanceTraveled >= maxDistance)
            Destroy(gameObject);
    }
}
