/// <summary>
/// 히트 시 사운드 재생이 필요한 HitBox가 선택적으로 구현
/// - IAttackHitBox와 분리(ISP) — 스윙 시점에 이미 사운드를 재생하는
///   SwordHitBox 같은 타입까지 강제로 구현하게 만들지 않기 위함
/// - EnemyHurtBox에서 GetComponent로 존재 여부만 확인해 사용
/// </summary>
public interface IHitSoundProvider
{
    SoundType HitSound { get; }
}
