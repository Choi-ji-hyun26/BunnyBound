/// <summary>
/// 검사 쉴드에 막힐 수 있는 객체 인터페이스
/// - ShieldHitBox.OnTriggerStay2D에서 감지 시 BlockedByShield() 호출
/// - EnemyBase 비의존 — 독립 투사체(FireBall 등)에서 구현
///
/// [구현체]
/// FireBall: 쉴드에 막히면 소멸
/// </summary>
public interface IShieldBlockable
{
    void BlockedByShield();
}
