using UnityEngine;

/// <summary>
/// 적 피격 HurtBox
/// - 항상 활성화
/// - PlayerHitBox(SwordHitBox) 레이어 감지 시 TakeDamage() 호출
/// - Layer: EnemyHurtBox / IsTrigger: ON
/// </summary>
public class EnemyHurtBox : MonoBehaviour
{
    private EnemyBase enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyBase>();
        if (enemy == null)
            Debug.LogError("[EnemyHurtBox] EnemyBase를 찾을 수 없습니다.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerHitBox")) return;

        SwordHitBox swordHitBox = other.GetComponent<SwordHitBox>();
        if (swordHitBox != null && enemy != null)
            enemy.TakeDamage(swordHitBox.Damage);
    }
}
