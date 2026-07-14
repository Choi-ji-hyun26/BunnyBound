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

    // 참고: PlayerSwordAttackHandler는 이 config를 이 프로퍼티로 거치지 않고
    // 자체적으로 동일한 에셋을 따로 참조 (damageConfig 필드)
    // 이유: PlayerSwordAttackHandler.Awake()가 데미지를 초기화해야 하는데
    // 그 시점에 Instance가 아직 세팅되기 전일 수 있음(Awake 순서 미보장)
    // 즉 두 참조가 존재하는 건 중복이 아닌 의도된 분리임

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
    /// tier마다 다른 데미지 계산(BASIC/WIND)이 공통으로 필요로 하는 
    /// "다음 tier / 만렙 여부" 계산을 한 곳에서만 처리
    /// </summary>
    private (int nextTier, bool isMax) GetNextTierInfo()
    {
        if (config == null) return (CurrentTier, true);

        bool isMax = CurrentTier >= config.MaxTier;
        int next = isMax ? CurrentTier : CurrentTier + 1;
        return (next, isMax);
    }

    /// <summary>
    /// BASIC(Q) 데미지 미리보기 — 현재 tier 수치 / 다음 tier 수치 / 만렙 여부
    /// </summary>
    public (int current, int next, bool isMax) GetBasicDamagePreview()
    {
        if (config == null) return (0, 0, true);
        var (next, isMax) = GetNextTierInfo();
        return (config.GetDamage1(CurrentTier), config.GetDamage1(next), isMax);
    }

    /// <summary>
    /// WIND(W) 데미지 미리보기 — GetBasicDamagePreview와 동일한 이유
    /// 해금 여부 판단은 포함하지 않음 — 호출부(WeaponUpgradePanel)가
    /// SkillUnlockManager/GameProgress를 통해 별도로 판단
    /// </summary>
    public (int current, int next, bool isMax) GetWindDamagePreview()
    {
        if (config == null) return (0, 0, true);
        var (next, isMax) = GetNextTierInfo();
        return (config.GetDamage2(CurrentTier), config.GetDamage2(next), isMax);
    }

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
