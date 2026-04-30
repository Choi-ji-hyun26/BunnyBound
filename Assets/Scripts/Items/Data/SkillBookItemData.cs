using UnityEngine;

/// <summary>
/// 스킬북 전용 ItemData
/// - ItemData를 상속해 attackIndex 필드 추가
/// - 기존 ItemData, CollectibleItem, OpenChest 코드 변경 없음
/// - GetEffectInstance() override로 attackIndex를 SkillBookEffect에 주입
///
/// [ScriptableObject 생성]
/// 우클릭 → Create → GameData → SkillBookItemData
/// attackIndex: 2(W), 3(E), 4(R)
/// </summary>
[CreateAssetMenu(fileName = "SkillBook", menuName = "GameData/SkillBookItemData")]
public class SkillBookItemData : ItemData
{
    [Header("스킬 해금")]
    [Tooltip("해금할 공격 인덱스: 2(W), 3(E), 4(R)")]
    public int attackIndex = 2;

    public override IItemEffect GetEffectInstance()
    {
        return new SkillBookEffect(attackIndex);
    }
}
