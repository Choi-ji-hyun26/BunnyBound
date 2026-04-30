using UnityEngine;

/// <summary>
/// 플레이어 피격 HurtBox
/// - 항상 활성화
/// - EnemyHitBox, Spike 레이어 감지 시 HandleHit() 호출
/// - 피격 판정(hitCooldown, isDamageInvincible)을 일괄 처리
/// - EnemyHitBox.OnTriggerEnter2D에서도 HandleHit()을 직접 호출 → 모든 피격이 이 곳을 통과
///
/// [피격 판정 흐름]
/// EnemyHitBox.OnTriggerEnter2D → HandleHit()
/// PlayerHurtBox.OnTriggerStay2D → HandleHit()
///   → hitCooldown 체크 (Stay 중복 + 무적 해제 직후 피격 방지)
///   → isDamageInvincible / isUnBeatTime 체크
///   → OnDamaged() 호출
///
/// hitCooldown = invincibleDuration + hitCooldownBuffer
/// → 스프라이트 복원(invincibleDuration) 후 buffer만큼 더 피격을 막아
/// → 시각적으로 색이 돌아온 후에 피격 가능 느낌을 보장하기 위함
/// </summary>
public class PlayerHurtBox : MonoBehaviour
{
    private PlayerDamageHandler damageHandler;
    private PlayerFeverHandler feverHandler;

    private int enemyHitBoxLayer;
    private int spikeLayer;

    [SerializeField] private float hitCooldownBuffer = 0.2f;

    private float hitCooldown;
    private float lastHitTime = -999f;

    private void Awake()
    {
        damageHandler = GetComponentInParent<PlayerDamageHandler>();
        feverHandler = GetComponentInParent<PlayerFeverHandler>();

        if (damageHandler == null)
            Debug.LogError("[PlayerHurtBox] PlayerDamageHandler를 찾을 수 없습니다.");
        if (feverHandler == null)
            Debug.LogError("[PlayerHurtBox] PlayerFeverHandler를 찾을 수 없습니다.");

        enemyHitBoxLayer = LayerMask.NameToLayer("EnemyHitBox");
        spikeLayer = LayerMask.NameToLayer("Spike");
    }

    private void Start()
    {
        hitCooldown = damageHandler.invincibleDuration + hitCooldownBuffer;
    }

    // ───────────────────────────────────────────
    // Stay — Piranha sleep 상태 대응 + Slug/Bee 접촉 지속 피격
    // ───────────────────────────────────────────
    private void OnTriggerStay2D(Collider2D other)
    {
        int layer = other.gameObject.layer;
        if (layer != enemyHitBoxLayer && layer != spikeLayer) return;

        HandleHit(other.transform.position);
    }

    // ───────────────────────────────────────────
    // 모든 피격이 통과하는 단일 진입점
    // EnemyHitBox.OnTriggerEnter2D에서도 직접 호출
    // ───────────────────────────────────────────
    public void HandleHit(Vector2 hitPosition)
    {
        // hitCooldown — Stay 중복 방지 + 스프라이트 복원 후 buffer 보장
        if (Time.time - lastHitTime < hitCooldown) return;

        // 무적 상태 체크
        if (damageHandler.isDamageInvincible || feverHandler.isUnBeatTime) return;

        lastHitTime = Time.time;
        damageHandler.OnDamaged(hitPosition);
    }
}
