using UnityEngine;

/// <summary>
/// 적 공격 HitBox
/// - 항상 활성화 (적이 살아있는 동안)
/// - PlayerHurtBox 레이어 감지 시 플레이어에게 데미지
/// - Layer: EnemyHitBox / IsTrigger: ON
/// </summary>
public class EnemyHitBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerHurtBox")) return;

        PlayerDamageHandler damageHandler = other.GetComponentInParent<PlayerDamageHandler>();
        if (damageHandler != null)
            damageHandler.OnDamaged(transform.position);
    }
}
