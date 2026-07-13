using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 무기 강화 UI 패널 — StageSelect 씬 배치
/// - 강화 진행률(현재 잔액/필요 개수) 표시
/// - 강화 버튼 클릭 시 WeaponUpgradeManager.TryUpgrade() 호출
/// - 강화 성공 시 VFX/SFX 재생 + 알림 표시
/// </summary>
public class WeaponUpgradePanel : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI progressText; // 예: "12 / 15"
    [SerializeField] private TextMeshProUGUI tierText;     // 예: "무기 Level 1"

    [Header("강화 성공 피드백")]
    [SerializeField] private ParticleSystem upgradeVFX;

    private void Start()
    {
        GameProgress.Load();
        RefreshUI();
    }

    private void OnEnable()
    {
        if (WeaponUpgradeManager.Instance != null)
            WeaponUpgradeManager.Instance.OnWeaponUpgraded += OnUpgraded;

        // 패널이 다시 열릴 때(예: 게임 씬에서 강화 후 스테이지 선택으로 복귀) 최신 상태 반영
        RefreshUI();
    }

    private void OnDisable()
    {
        if (WeaponUpgradeManager.Instance != null)
            WeaponUpgradeManager.Instance.OnWeaponUpgraded -= OnUpgraded;
    }

    // ───────────────────────────────────────────
    // 버튼 연결 — Unity UI Button OnClick()
    // ───────────────────────────────────────────
    public void OnUpgradeButtonClicked()
    {
        WeaponUpgradeManager.Instance?.TryUpgrade();
    }

    // ───────────────────────────────────────────
    // 강화 성공 콜백
    // ───────────────────────────────────────────
    private void OnUpgraded(int newTier)
    {
        SoundManager.Instance?.PlaySound(SoundType.WeaponUpgrade);
        if (upgradeVFX != null)
            upgradeVFX.Play();
        NotificationUI.Instance?.Show("무기가 강화되었습니다!");

        RefreshUI();
    }

    // ───────────────────────────────────────────
    // UI 갱신
    // ───────────────────────────────────────────
    private void RefreshUI()
    {
        var mgr = WeaponUpgradeManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[WeaponUpgradePanel] WeaponUpgradeManager.Instance가 없습니다.");
            return;
        }

        int spendable = mgr.SpendableStars;
        int cost = mgr.CostForNextTier;

        if (cost < 0) // 만렙
        {
            if (progressText != null) progressText.text = "MAX";
            if (upgradeButton != null) upgradeButton.interactable = false;
        }
        else
        {
            if (progressText != null) progressText.text = $"{spendable} / {cost}";
            if (upgradeButton != null) upgradeButton.interactable = mgr.CanUpgrade;
        }

        if (tierText != null)
            tierText.text = $"무기 Level {mgr.CurrentTier}";
    }
}
