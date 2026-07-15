using UnityEngine;

/// <summary>
/// 피버 상태 전용 HitBox
/// - 피버 중에만 활성화
/// - 몸통 크기와 동일한 Trigger 콜라이더
/// - EnemyHurtBox가 PlayerHitBox 레이어를 감지해서 TakeDamage() 자동 호출
/// - Layer: PlayerHitBox / IsTrigger: ON
/// </summary>
public class FeverHitBox : MonoBehaviour, IAttackHitBox, IHitSoundProvider
{
    [SerializeField] private int damage = 100; 
    public int Damage => damage; // IAttackHitBox 구현

    public SoundType HitSound => SoundType.FeverHit; // IHitSoundProvider 구현

    private Collider2D hitCollider;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        // 시작 시 비활성화
        if (hitCollider != null)
            hitCollider.enabled = false;
    }

    /// <summary>
    /// 피버 시작 시 PlayerFeverHandler에서 호출
    /// </summary>
    public void Activate()
    {
        if (hitCollider != null)
            hitCollider.enabled = true;
    }

    /// <summary>
    /// 피버 종료 시 PlayerFeverHandler에서 호출
    /// </summary>
    public void Deactivate()
    {
        if (hitCollider != null)
            hitCollider.enabled = false;
    }
}
