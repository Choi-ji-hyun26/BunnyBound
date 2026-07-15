using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 무기 강화 UI 패널 — StageSelect/Game 씬 공용 (버튼으로 여는 모달)
/// - 강화 진행률(현재 잔액/필요 개수) 표시
/// - Q/W 공격력 스탯 표시 — W는 스테이지 5 힌트 상자(옵션)로 해금되므로
///   미해금 상태에서는 잠금 안내로 대체 (레이아웃 고정, 정보 완결성 확보)
/// - 강화 버튼 클릭 시 WeaponUpgradeManager.TryUpgrade() 호출
/// - 강화 성공 시 VFX/SFX 재생 + 알림 표시
/// - 모달 열림/닫힘 시 Time.timeScale 제어 — 배경 오버레이(Raycast 차단)가
///   다른 모달과의 동시 오픈을 막아주므로 별도 상호 배제 코드 없이 안전
/// - 데미지 수치 계산(다음 tier 값 등)은 WeaponUpgradeManager에 위임 —
///   이 클래스는 그 결과를 받아 포맷팅만 담당
/// </summary>
public class WeaponUpgradePanel : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI progressText; // 예: "12 / 15"
    [SerializeField] private TextMeshProUGUI tierText;     // 예: "Lv. 1"
    [SerializeField] private TextMeshProUGUI basicStatText; // 예: "BASIC +8 >> +10"
    [SerializeField] private TextMeshProUGUI windStatText;  // 예: "WIND +13 >> +15" 또는 잠금 안내

    [Header("텍스트 강조 색상 (리치 텍스트 태그)")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.3f); // 강화 후 수치, 별 잔액 강조
    [SerializeField] private Color lockedColor = new Color(0.6f, 0.6f, 0.6f);   // 미해금 안내 텍스트

    [Header("강화 성공 피드백")]
    [SerializeField] private UpgradeFlipbookEffect upgradeVFX;

    private const int WindAttackIndex = 2; // SkillUnlockManager 기준 W 공격 인덱스

    private bool isValid; // 필수 참조 검증 결과 — 릴리즈 빌드에서도 유지되어야 하므로 Debug.Assert 대신 일반 검사 사용

    private void Awake()
    {
        // 필수 참조는 여기서 한 번에 검증 — Debug.LogError는 릴리즈 빌드에서도 남아있어
        // 기록을 남김, 대신 크래시(NullReferenceException) 대신 기능만 조용히 비활성화되도록 방어
        // (upgradeVFX는 선택 사항이라 제외 — 없어도 강화 자체는 정상 동작해야 함)
        isValid = upgradeButton != null && progressText != null && tierText != null
            && basicStatText != null && windStatText != null;

        if (!isValid)
            Debug.LogError("[WeaponUpgradePanel] 필수 UI 참조가 하나 이상 연결되지 않았습니다.");
    }

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
        NotificationUI.Instance?.Show("Weapon Level Up!");

        RefreshUI();
    }

    // ───────────────────────────────────────────
    // UI 갱신
    // ───────────────────────────────────────────
    private void RefreshUI()
    {
        if (!isValid) return;

        var mgr = WeaponUpgradeManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[WeaponUpgradePanel] WeaponUpgradeManager.Instance가 없습니다.");
            return;
        }

        int tier = mgr.CurrentTier;

        RefreshProgress(mgr);
        RefreshStatLines(mgr);

        tierText.text = $"Lv. {tier + 1}"; // tier 0(기본)을 Lv.1로 표시 — 게임 관례상 1-indexed
    }

    private void RefreshProgress(WeaponUpgradeManager mgr)
    {
        int cost = mgr.CostForNextTier;

        if (cost < 0) // 만렙
        {
            progressText.text = "MAX";
            upgradeButton.interactable = false;
            return;
        }

        progressText.text = $"{Hex(mgr.SpendableStars, highlightColor)} / {cost}";
        upgradeButton.interactable = mgr.CanUpgrade;
    }

    private void RefreshStatLines(WeaponUpgradeManager mgr)
    {
        // BASIC — Q는 항상 해금 상태이므로 조건 없이 표시
        var (curBasic, nextBasic, isMax) = mgr.GetBasicDamagePreview();
        basicStatText.text = isMax
            ? $"BASIC +{curBasic}"
            : $"BASIC +{curBasic} >> {Hex($"+{nextBasic}", highlightColor)}";

        // WIND — 스테이지 5 힌트 상자(옵션)로 해금되므로 미해금 시 잠금 안내로 대체
        // SkillUnlockManager.Instance가 없는 씬(예: StageSelect)에서도 GameProgress 직접 조회로
        // 정확한 해금 상태를 판단 — "매니저 없음"을 "미해금"이라고 오판하지 않도록
        bool windUnlocked = SkillUnlockManager.Instance != null
            ? SkillUnlockManager.Instance.IsUnlocked(WindAttackIndex)
            : GameProgress.IsSkillUnlocked(WindAttackIndex);

        if (!windUnlocked)
        {
            windStatText.text = Hex("???? +?? >> +??", lockedColor);
            return;
        }

        var (curWind, nextWind, _) = mgr.GetWindDamagePreview();
        windStatText.text = isMax
            ? $"WIND +{curWind}"
            : $"WIND +{curWind} >> {Hex($"+{nextWind}", highlightColor)}";
    }

    // ───────────────────────────────────────────
    // TMP 리치 텍스트 색상 태그 헬퍼
    // ───────────────────────────────────────────
    private static string Hex(object value, Color color) =>
        $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{value}</color>";
}
