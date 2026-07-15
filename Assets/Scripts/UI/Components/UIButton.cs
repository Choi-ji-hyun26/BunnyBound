using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 클릭 시 공통 SFX를 재생하는 Button
/// - 프로젝트 표준 버튼 — 새 버튼은 이 클래스(또는 이 클래스를 쓴 프리팹)에서 파생
/// - 사운드는 SoundManager의 SoundType.ButtonClick 하나로 통일
///   (버튼마다 다른 사운드가 필요해지면 IHitSoundProvider처럼 별도 인터페이스로 분리 검토)
/// </summary>
public class UIButton : Button
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;

        base.OnPointerClick(eventData);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(SoundType.ButtonClick);
    }
}
