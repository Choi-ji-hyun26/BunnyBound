using UnityEngine;

/// <summary>
/// 힌트 아이템 전용 ItemData
/// - ItemData를 상속해 hintId, hintText 필드 추가
/// - 기존 ItemData, CollectibleItem, OpenChest 코드 변경 없음
/// - GetEffectInstance() override로 hintId, hintText를 HintEffect에 주입
///
/// [ScriptableObject 생성]
/// 우클릭 → Create → GameData → HintItemData
///
/// [인스펙터 설정]
/// - isOneTimeItem: true
/// - hintId: 챕터번호 * 100 + 힌트번호 (예: 챕터1 힌트1 → 101, 힌트2 → 102)
/// - hintText: 동화 스타일 힌트 텍스트 (심볼 순서를 암시)
/// </summary>
[CreateAssetMenu(fileName = "HintItem", menuName = "GameData/HintItemData")]
public class HintItemData : ItemData
{
    [Header("힌트")]
    [Tooltip("챕터번호 * 100 + 힌트번호 (예: 챕터1 힌트1 → 101)")]
    public int hintId;

    [TextArea]
    [Tooltip("심볼 순서를 암시하는 동화 스타일 텍스트")]
    public string hintText;

    public override IItemEffect GetEffectInstance()
    {
        return new HintEffect(hintId, hintText);
    }
}
