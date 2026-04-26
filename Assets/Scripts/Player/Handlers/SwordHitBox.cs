using UnityEngine;

/// <summary>
/// 검사 공격 HitBox
/// - PlayerSwordAttackHandler가 활성화/비활성화 제어
/// - EnemyHurtBox 레이어 감지 시 데미지 전달
/// - 파괴 가능 오브젝트(Breakable) 감지 시 OnBreak() 호출
/// - Layer: PlayerHitBox / IsTrigger: ON
/// </summary>
public class SwordHitBox : MonoBehaviour
{
    private int damage = 10;
    public int Damage => damage; // EnemyHurtBox에서 참조

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적 HurtBox 감지 → EnemyHurtBox에서 TakeDamage 처리

        // 파괴 가능 오브젝트 처리 (퍼즐용)
        if (other.CompareTag("Breakable"))
        {
            IBreakable breakable = other.GetComponent<IBreakable>();
            breakable?.OnBreak();
        }
    }
}
