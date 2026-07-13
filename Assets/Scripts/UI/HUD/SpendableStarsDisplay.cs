using UnityEngine;
using TMPro;

/// <summary>
/// 큰별(spendableStars) 잔액 상시 표시 위젯 — 화면 좌상단 등에 배치
/// - WeaponUpgradeManager.OnWeaponUpgraded 이벤트로 강화 소비 시 실시간 갱신
/// - 모달(강화창)과 달리 항상 활성 상태이므로 OnEnable 1회 + 이벤트 구독 조합
/// </summary>
public class SpendableStarsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starCountText;

    private void OnEnable()
    {
        if (WeaponUpgradeManager.Instance != null)
            WeaponUpgradeManager.Instance.OnWeaponUpgraded += OnStarsChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (WeaponUpgradeManager.Instance != null)
            WeaponUpgradeManager.Instance.OnWeaponUpgraded -= OnStarsChanged;
    }

    private void OnStarsChanged(int newTier) => Refresh();

    private void Refresh()
    {
        // WeaponUpgradeManager.Instance가 아직 세팅되지 않은 시점(Awake 순서 미보장)에도
        // GameProgress 직접 조회로 정확한 값을 표시 — 기존에 여러 번 겪은
        // "매니저 부재 = 잘못된 값"을 반복하지 않기 위한 폴백
        int spendable = WeaponUpgradeManager.Instance != null
            ? WeaponUpgradeManager.Instance.SpendableStars
            : GameProgress.GetSpendableStars();

        if (starCountText != null)
            starCountText.text = spendable.ToString();
    }
}
