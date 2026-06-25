/// <summary>
/// 힌트 아이템 효과
/// - 힌트 상자에서 획득 시 GameProgress에 hintId 기록
/// - HintSlotUI 갱신 및 HintDetailUI 팝업 표시
/// - HintItemData.GetEffectInstance()에서 hintId, hintText 주입
/// </summary>
public class HintEffect : IItemEffect
{
    private readonly int hintId;
    private readonly string hintText;

    public HintEffect(int hintId, string hintText)
    {
        this.hintId = hintId;
        this.hintText = hintText;
    }

    public void ApplyEffect(PlayerCoordinator player)
    {
        // 1. GameProgress에 힌트 획득 기록
        GameProgress.CollectHint(hintId);

        // 2. HintListUI 갱신
        HintListUI.Instance?.Refresh();

        // 3. 힌트 상세 팝업 즉시 표시
        HintDetailUI.Instance?.Show(hintId, hintText);

        // 4. 아이템 획득 사운드
        SoundManager.Instance.PlaySound(SoundType.Item);
    }
}
