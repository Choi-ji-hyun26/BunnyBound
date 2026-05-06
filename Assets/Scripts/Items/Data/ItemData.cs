using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "GameData/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject itemPrefab;

    // 1회용 아이템 여부 — HPUp, SkillBook은 true, 나머지는 false
    // Inspector에서 해당 ScriptableObject에 체크
    public bool isOneTimeItem = false;

    // 이 아이템이 사용할 효과 클래스의 타입을 저장
    [SerializeField]
    private string itemEffectClassName;

    // 저장된 문자열 이름을 바탕으로 실제 효과 인스턴스를 생성
    // virtual — SkillBookItemData에서 override해서 attackIndex 주입
    public virtual IItemEffect GetEffectInstance()
    {
        System.Type effectType = System.Type.GetType(itemEffectClassName);
        if (effectType != null && typeof(IItemEffect).IsAssignableFrom(effectType))
        {
            return (IItemEffect)System.Activator.CreateInstance(effectType);
        }
        return null;
    }
}
