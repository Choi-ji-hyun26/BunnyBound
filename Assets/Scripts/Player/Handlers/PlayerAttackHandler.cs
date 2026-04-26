using UnityEngine;

/// <summary>
/// 토끼 점프 공격 제거됨
/// - 공격/피격은 HitBox/HurtBox 구조로 통일
/// - 토끼: 공격 없음 (이동/탐색 특화)
/// - 검사: PlayerSwordAttackHandler에서 처리
/// </summary>
public class PlayerAttackHandler : MonoBehaviour
{
    // 점프 공격 제거
    // 피격은 PlayerHurtBox.cs에서 처리
    // 검사 공격은 PlayerSwordAttackHandler.cs에서 처리
}
