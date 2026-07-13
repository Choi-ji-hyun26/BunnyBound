using UnityEngine;

/// <summary>
/// 무기(검) 강화 시스템
/// - 강화 재화: 스테이지 클리어로 적립되는 소비형 큰별(spendableStars)
/// - 강화 비용은 WeaponUpgradeConfig(ScriptableObject)에서 관리 — 티어별 비용 테이블
/// - StageSelect 씬, Game 씬 양쪽에 배치 — GameProgress(static)가 진실 공급원이므로
///   DontDestroyOnLoad 없이 씬마다 재생성되어도 데이터 일관성 문제 없음
/// </summary>
public class WeaponUpgradeManager : MonoBehaviour
{
    public static WeaponUpgradeManager Instance { get; private set; }

    [SerializeField] private WeaponUpgradeConfig config;

    public WeaponUpgradeConfig Config => config;

    // 강화 성공 시 새 tier 전달 — UI 갱신, VFX/SFX 재생 등에서 구독
    public event System.Action<int> OnWeaponUpgraded;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (config == null)
            Debug.LogError("[WeaponUpgradeManager] config가 연결되지 않았습니다.");

        GameProgress.Load();
    }

    public int CurrentTier => GameProgress.GetWeaponUpgradeTier();

    public int SpendableStars => GameProgress.GetSpendableStars();

    /// <summary>
    /// 다음 강화에 필요한 큰별 개수, 이미 만렙이거나 config 미연결이면 -1
    /// </summary>
    public int CostForNextTier => config != null ? config.GetCost(CurrentTier) : -1;

    public bool CanUpgrade =>
        config != null && CurrentTier < config.MaxTier && SpendableStars >= CostForNextTier;

    /// <summary>
    /// 강화 시도 — 성공 시 큰별 차감 + tier 증가 + 즉시 저장 + 이벤트 발행
    /// 실패 조건: 이미 만렙, config 미연결, 또는 큰별 잔액 부족
    /// </summary>
    public bool TryUpgrade()
    {
        if (!CanUpgrade) return false;

        int cost = CostForNextTier;
        if (!GameProgress.TrySpendStars(cost)) return false; // CanUpgrade와 중복 검증 — 방어적 이중 체크

        int newTier = CurrentTier + 1;
        GameProgress.SetWeaponUpgradeTier(newTier);

        // 스냅샷/롤백 경로를 타지 않는 즉시 확정 저장
        // spendableStars는 이미 클리어를 완료한 과거 스테이지의 확정 재화이므로
        // 현재 진행 중인 스테이지 시도의 성공/실패와 무관하게 영구 반영되어야 함
        GameProgress.SaveImmediate();

        OnWeaponUpgraded?.Invoke(newTier);
        return true;
    }
}
