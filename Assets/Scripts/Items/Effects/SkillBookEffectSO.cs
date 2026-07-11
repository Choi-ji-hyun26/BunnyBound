using UnityEngine;

/// <summary>
/// 스킬북 아이템 효과
/// - 보물상자에서 획득 시 해당 attackIndex 공격 해금
/// - 획득 시 NotificationUI로 화면 상단 중앙에 해금 메시지 표시
///
/// [인스펙터 설정]
/// attackIndex: 2(W), 3(E), 4(R)
/// </summary>
[CreateAssetMenu(fileName = "SkillBookEffect", menuName = "GameData/Effects/SkillBookEffect")]
public class SkillBookEffectSO : ItemEffectSO
{
    [Header("스킬 해금")]
    [Tooltip("해금할 공격 인덱스: 2(W), 3(E), 4(R)")]
    [SerializeField] private int attackIndex = 2;

    // attackIndex별 해금 메시지
    private static readonly string[] unlockMessages =
    {
        "",                                         // 0 (미사용)
        "",                                         // 1 (기본 공격, 해금 불필요)
        "NEW SKILL UNLOCKED!\nTRY PRESSING W!",     // 2
    };

    public override void ApplyEffect(PlayerCoordinator player)
    {
        if (SkillUnlockManager.Instance == null)
        {
            Debug.LogError("[SkillBookEffectSO] SkillUnlockManager가 없습니다.");
            return;
        }

        SkillUnlockManager.Instance.UnlockAttack(attackIndex);
        SoundManager.Instance.PlaySound(SoundType.Item);

        if (NotificationUI.Instance != null && attackIndex < unlockMessages.Length)
            NotificationUI.Instance.Show(unlockMessages[attackIndex]);

        Debug.Log($"[SkillBookEffectSO] Attack{attackIndex} 해금!");
    }
}
