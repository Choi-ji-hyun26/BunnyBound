using UnityEngine;

/// <summary>
/// 아이템 데이터
/// - 효과는 ItemEffectSO 에셋을 직접 참조 (ScriptableObject 기반 Strategy)
/// - 새 효과 추가 시 ItemData/CollectibleItem 수정 없이 ItemEffectSO 서브클래스 +
///   에셋만 추가하면 됨 (Open-Closed 원칙)
/// </summary>
[CreateAssetMenu(fileName = "NewItemData", menuName = "GameData/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject itemPrefab;

    // 1회용 아이템 여부 — HPUp, SkillBook은 true, 나머지는 false
    // Inspector에서 해당 ScriptableObject에 체크
    public bool isOneTimeItem = false;

    [Tooltip("이 아이템 획득 시 적용할 효과 에셋 (ItemEffectSO 상속 클래스)")]
    public ItemEffectSO effect;

    // CollectibleItem 등 기존 호출부와의 호환을 위한 접근자
    public IItemEffect GetEffectInstance() => effect;
}
