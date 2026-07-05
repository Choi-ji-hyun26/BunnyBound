using UnityEngine;

/// <summary>
/// FlyingDemon 원거리 투사체
/// - EnemyHitBox 레이어 → PlayerHurtBox 감지 → 플레이어 데미지 1
/// - IShieldBlockable 구현 → 검사 쉴드에 막히면 소멸
/// - 수평 이동, maxDistance 도달 시 소멸
/// - Rigidbody2D.gravityScale = 0 (공중 직선 이동)
///
/// [피격 흐름]
/// FireBall(EnemyHitBox) → PlayerHurtBox.OnTriggerStay2D 감지
/// → HandleHit(pos, damageAmount) → PlayerDamageHandler.OnDamaged()
///
/// [쉴드 차단 흐름]
/// FireBall(EnemyHitBox) → ShieldHitBox.OnTriggerStay2D 감지
/// → IShieldBlockable.BlockedByShield() → Destroy(gameObject)
///
/// [Inspector 설정]
/// - moveSpeed: 발사 속도
/// - maxDistance: 최대 이동 거리 (감지 범위보다 약간 길게)
/// </summary>
public class FireBall : MonoBehaviour, IShieldBlockable
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float maxDistance = 8f;

    private float direction = 1f;
    private Vector2 startPosition;
    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// FlyingDemonAttackState에서 생성 직후 호출
    /// </summary>
    public void Initialize(float direction)
    {
        this.direction = direction;
        startPosition = transform.position;

        Vector3 scale = transform.localScale;
        scale.x = direction < 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void FixedUpdate()
    {
        rigid.velocity = new Vector2(direction * moveSpeed, 0f);

        float distanceTraveled = Mathf.Abs(transform.position.x - startPosition.x);
        if (distanceTraveled >= maxDistance)
            Destroy(gameObject);
    }

    /// <summary>
    /// 검사 쉴드에 막힐 시 ShieldHitBox에서 호출
    /// </summary>
    public void BlockedByShield()
    {
        Destroy(gameObject);
    }
}
