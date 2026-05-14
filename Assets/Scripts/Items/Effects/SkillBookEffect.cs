using UnityEngine;

/// <summary>
/// 스킬북 아이템 효과
/// - 보물상자에서 획득 시 해당 attackIndex 공격 해금
/// - SkillBookItemData.GetEffectInstance()에서 attackIndex 주입
/// - 획득 시 NotificationUI로 화면 상단 중앙에 해금 메시지 표시
/// </summary>
public class SkillBookEffect : IItemEffect
{
    private readonly int attackIndex;

    // attackIndex별 해금 메시지
    private static readonly string[] unlockMessages =
    {
        "",                                         // 0 (미사용)
        "",                                         // 1 (기본 공격, 해금 불필요)
        "NEW SKILL UNLOCKED!\nTRY PRESSING W!",     // 2
        "NEW SKILL UNLOCKED!\nTRY PRESSING E!",     // 3
        "NEW SKILL UNLOCKED!\nTRY PRESSING R!",     // 4
    };

    public SkillBookEffect(int attackIndex)
    {
        this.attackIndex = attackIndex;
    }

    public void ApplyEffect(PlayerCoordinator player)
    {
        if (SkillUnlockManager.Instance == null)
        {
            Debug.LogError("[SkillBookEffect] SkillUnlockManager가 없습니다.");
            return;
        }

        SkillUnlockManager.Instance.UnlockAttack(attackIndex);
        SoundManager.Instance.PlaySound(SoundType.Item);

        // 해금 메시지 표시
        if (NotificationUI.Instance != null && attackIndex < unlockMessages.Length)
            NotificationUI.Instance.Show(unlockMessages[attackIndex]);

        Debug.Log($"[SkillBookEffect] Attack{attackIndex} 해금!");
    }
}
