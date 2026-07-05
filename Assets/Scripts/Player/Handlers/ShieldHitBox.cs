using UnityEngine;

/// <summary>
/// 쉴드 전면 HitBox
/// - Player 전면에 배치 (플레이어 방향에 따라 offset 전환)
/// - 쉴드 중에만 활성화
/// - EnemyHitBox 레이어 감지 시:
///   1. IShieldBlockable 구현체(FireBall 등) → BlockedByShield() 호출 후 return
///   2. EnemyBase → 넉백/스턴
/// - PlayerShieldHandler에서 활성화/비활성화 제어
/// </summary>
public class ShieldHitBox : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private PlayerShieldHandler shieldHandler;

    [Header("Settings")]
    [SerializeField] private float shieldOffsetX = 0.8f;

    private BoxCollider2D shieldCollider;
    private int enemyHitBoxLayer;

    private void Awake()
    {
        shieldCollider = GetComponent<BoxCollider2D>();
        enemyHitBoxLayer = LayerMask.NameToLayer("EnemyHitBox");

        if (shieldCollider != null)
            shieldCollider.enabled = false;
    }

    private void Update()
    {
        if (playerSprite == null) return;

        float offsetX = playerSprite.flipX ? -shieldOffsetX : shieldOffsetX;
        transform.localPosition = new Vector3(offsetX, transform.localPosition.y, 0f);
    }

    public void Activate()
    {
        if (shieldCollider != null)
            shieldCollider.enabled = true;
    }

    public void Deactivate()
    {
        if (shieldCollider != null)
            shieldCollider.enabled = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != enemyHitBoxLayer) return;
        if (shieldHandler == null || !shieldHandler.IsShielding) return;

        // IShieldBlockable 체크 먼저 — FireBall 등 독립 투사체 처리
        IShieldBlockable blockable = other.GetComponent<IShieldBlockable>();
        if (blockable != null)
        {
            blockable.BlockedByShield();
            return;
        }

        // 기존 EnemyBase 처리
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;

        // 이미 넉백/스턴 중이면 아무것도 하지 않음
        // KnockbackRoutine이 돌아가는 동안 velocity를 건드리면 넉백이 즉시 무효화됨
        if (enemy.IsStunned) return;

        shieldHandler.ReactToEnemy(enemy);
    }
}
