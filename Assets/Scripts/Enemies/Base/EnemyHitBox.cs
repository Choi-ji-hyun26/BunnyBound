using UnityEngine;

/// <summary>
/// 적 공격 HitBox
/// - 항상 활성화 (적이 살아있는 동안)
/// - PlayerHurtBox 레이어 감지 시 PlayerHurtBox.HandleHit() 호출
/// - 피격 판정(hitCooldown, isDamageInvincible)은 PlayerHurtBox에서 일괄 처리
/// </summary>
public class EnemyHitBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerHurtBox")) return;

        PlayerHurtBox hurtBox = other.GetComponent<PlayerHurtBox>();
        if (hurtBox != null)
            hurtBox.HandleHit(transform.position);
    }
}
