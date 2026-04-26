/// <summary>
/// 공격 HitBox 공통 인터페이스
/// - SwordHitBox, FeverHitBox 모두 구현
/// - EnemyHurtBox에서 통일된 방식으로 데미지 처리
/// </summary>
public interface IAttackHitBox
{
    int Damage { get; }
}
