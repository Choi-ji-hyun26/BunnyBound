using UnityEngine;

/// <summary>
/// 적 공격 HitBox
/// - 항상 활성화 (적이 살아있는 동안)
/// - PlayerHurtBox 레이어 감지 시 PlayerHurtBox.HandleHit() 호출
/// - 피격 판정(hitCooldown, isDamageInvincible)은 PlayerHurtBox에서 일괄 처리
/// - 충돌한 적에게 OnHitPlayer() 콜백 전달 → 적별 충돌 반응 처리 (Bat 튕겨나가기 등)
/// </summary>
public class EnemyHitBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerHurtBox")) return;

        // 플레이어 피격 처리
        PlayerHurtBox hurtBox = other.GetComponent<PlayerHurtBox>();
        if (hurtBox != null)
            hurtBox.HandleHit(transform.position);

        // 적 충돌 반응 콜백 — EnemyBase.OnHitPlayer() virtual
        // Bat 등 충돌 후 튕겨나가는 적에서 override해서 사용
        EnemyBase enemy = GetComponentInParent<EnemyBase>();
        if (enemy != null)
            enemy.OnHitPlayer();
    }
}
