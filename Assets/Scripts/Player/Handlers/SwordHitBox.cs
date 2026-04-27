using UnityEngine;

/// <summary>
/// 검사 공격 HitBox
/// - PlayerSwordAttackHandler가 활성화/비활성화 제어
/// - EnemyHurtBox 레이어 감지 시 데미지 전달
/// - Breakable 처리는 PlayerSwordAttackHandler에서 OverlapBox로 직접 처리
/// - Layer: PlayerHitBox / IsTrigger: ON
/// </summary>
public class SwordHitBox : MonoBehaviour, IAttackHitBox
{
    private int damage = 10;
    public int Damage => damage; // IAttackHitBox 구현

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    // 적 피격은 EnemyHurtBox.OnTriggerEnter2D에서 처리
    // Breakable은 PlayerSwordAttackHandler.ExecuteAttack()에서 OverlapBox로 처리
}
