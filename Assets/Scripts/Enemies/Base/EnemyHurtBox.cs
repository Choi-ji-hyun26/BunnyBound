using UnityEngine;

/// <summary>
/// 적 피격 HurtBox
/// - 항상 활성화
/// - PlayerHitBox(SwordHitBox, FeverHitBox) 레이어 감지 시 TakeDamage() 호출
/// - hitPosition을 함께 전달 → EnemyBase에서 넉백/스턴 방향 계산에 사용
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

        IAttackHitBox attackHitBox = other.GetComponent<IAttackHitBox>();
        if (attackHitBox == null || enemy == null) return;

        enemy.TakeDamage(attackHitBox.Damage, other.transform.position);

        IHitSoundProvider soundProvider = other.GetComponent<IHitSoundProvider>();
        if (soundProvider != null)
            SoundManager.Instance.PlaySound(soundProvider.HitSound);
    }
}
