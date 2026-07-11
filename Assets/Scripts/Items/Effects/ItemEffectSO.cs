using UnityEngine;

/// <summary>
/// 아이템 효과 베이스 (ScriptableObject 기반 Strategy)
///
/// [설계 배경]
/// 기존에는 ItemData.itemEffectClassName(문자열) + 리플렉션으로 효과 인스턴스를
/// 생성했으나 클래스명 오타/리네이밍 시 컴파일 에러 없이 조용히 깨지는 문제가 존재함
/// 매개변수가 필요한 효과(Hint, SkillBook)는 이 방식으로 생성자 호출이 불가능해
/// ItemData를 상속한 서브클래스에서 GetEffectInstance()를 override하는 별도 경로를
/// 써야 했고 결과적으로 같은 문제(효과 생성)를 두 가지 방식으로 처리하고 있었음
///
/// 이 클래스로 통일: 효과 자체를 ScriptableObject 에셋으로 만들어
/// ItemData가 effect 필드로 직접 참조하도록 변경
///  새 효과 추가 시 ItemData/CollectibleItem 수정 없이 이 클래스를 상속한 새 에셋만 추가하면 됨
/// </summary>
public abstract class ItemEffectSO : ScriptableObject, IItemEffect
{
    public abstract void ApplyEffect(PlayerCoordinator player);
}
