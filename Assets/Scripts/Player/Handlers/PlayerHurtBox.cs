using UnityEngine;

/// <summary>
/// 플레이어 피격 HurtBox
/// - 항상 활성화
/// - EnemyHitBox 레이어 감지 시 OnDamaged() 호출
/// - 피격 무적(isDamageInvincible) 또는 피버 무적(isUnBeatTime) 중이면 피격 무시
/// - Layer: PlayerHurtBox / IsTrigger: ON
/// </summary>
public class PlayerHurtBox : MonoBehaviour
{
    private PlayerDamageHandler damageHandler;
    private PlayerFeverHandler feverHandler;

    private void Awake()
    {
        damageHandler = GetComponentInParent<PlayerDamageHandler>();
        feverHandler = GetComponentInParent<PlayerFeverHandler>();

        if (damageHandler == null)
            Debug.LogError("[PlayerHurtBox] PlayerDamageHandler를 찾을 수 없습니다.");
        if (feverHandler == null)
            Debug.LogError("[PlayerHurtBox] PlayerFeverHandler를 찾을 수 없습니다.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("EnemyHitBox")) return;

        // 피격 무적 or 피버 무적 중이면 피격 무시
        if (damageHandler.isDamageInvincible || feverHandler.isUnBeatTime) return;

        if (damageHandler != null)
            damageHandler.OnDamaged(other.transform.position);
    }
}
