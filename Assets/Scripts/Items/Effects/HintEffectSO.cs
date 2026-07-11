using UnityEngine;

/// <summary>
/// 힌트 아이템 효과
/// - 힌트 상자에서 획득 시 GameProgress에 hintId 기록
/// - HintListUI 갱신 및 HintDetailUI 팝업 표시
/// - HintListUI/HintDetailUI가 hintId로 텍스트를 역참조하므로
///   HintId/HintText를 public 프로퍼티로 노출 (읽기 전용)
///
/// [인스펙터 설정]
/// - hintId: 챕터번호 * 100 + 힌트번호 (예: 챕터1 힌트1 → 101, 힌트2 → 102)
/// - hintText: 동화 스타일 힌트 텍스트 (심볼 순서를 암시)
/// </summary>
[CreateAssetMenu(fileName = "HintEffect", menuName = "GameData/Effects/HintEffect")]
public class HintEffectSO : ItemEffectSO
{
    [Header("힌트")]
    [Tooltip("챕터번호 * 100 + 힌트번호 (예: 챕터1 힌트1 → 101)")]
    [SerializeField] private int hintId;

    [TextArea]
    [Tooltip("심볼 순서를 암시하는 동화 스타일 텍스트")]
    [SerializeField] private string hintText;

    public int HintId => hintId;
    public string HintText => hintText;

    public override void ApplyEffect(PlayerCoordinator player)
    {
        GameProgress.CollectHint(hintId);
        HintListUI.Instance?.Refresh();
        HintDetailUI.Instance?.Show(hintId, hintText);
        SoundManager.Instance.PlaySound(SoundType.Item);
    }
}
