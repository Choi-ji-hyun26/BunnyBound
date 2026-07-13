using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 무기 강화 UI 패널 — StageSelect/Game 씬 공용 (버튼으로 여는 모달)
/// - 강화 진행률(현재 잔액/필요 개수) 표시
/// - 강화 버튼 클릭 시 WeaponUpgradeManager.TryUpgrade() 호출
/// - 강화 성공 시 VFX/SFX 재생 + 알림 표시
/// - 모달 열림/닫힘 시 Time.timeScale 제어 — 배경 오버레이(Raycast 차단)가
///   다른 모달과의 동시 오픈을 막아주므로 별도 상호 배제 코드 없이 안전
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

        // 모달 열림 시 일시정지 — StageSelect/Game 씬 어디에 배치되든 동일하게 적용
        // 배경 오버레이가 다른 모달과의 동시 오픈을 막아주므로
        // 별도의 상호 배제 코드(ModalGate 등) 없이 안전
        Time.timeScale = 0f;

        // 패널이 다시 열릴 때(예: 게임 씬에서 강화 후 스테이지 선택으로 복귀) 최신 상태 반영
        RefreshUI();
    }

    private void OnDisable()
    {
        if (WeaponUpgradeManager.Instance != null)
            WeaponUpgradeManager.Instance.OnWeaponUpgraded -= OnUpgraded;

        Time.timeScale = 1f;
    }

    // ───────────────────────────────────────────
    // 열기/닫기 — HUD 진입 버튼, 패널 내부 닫기 버튼에서 호출
    // ───────────────────────────────────────────
    public void Open() => gameObject.SetActive(true);

    public void Close() => gameObject.SetActive(false);

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
