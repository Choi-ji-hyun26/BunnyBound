using UnityEngine;

/// <summary>
/// 무기 강화 데이터 테이블 (강화 비용 + 공격력)
/// - tierCosts[i]: (i+1)차 강화에 필요한 큰별(spendableStars) 개수
/// - 배열 길이가 곧 최대 강화 단계(MaxTier) — 별도 상수와 어긋날 위험 없음
/// - StageSelect/Game 씬의 WeaponUpgradeManager, PlayerSwordAttackHandler,
///   WeaponUpgradePanel이 모두 이 에셋 하나를 공유 참조 — 데미지/비용 수치의
///   단일 진실 공급원 (씬/컴포넌트마다 값이 따로 존재해 어긋나는 것을 방지)
/// </summary>
[CreateAssetMenu(fileName = "WeaponUpgradeConfig", menuName = "BunnyBound/WeaponUpgradeConfig")]
public class WeaponUpgradeConfig : ScriptableObject
{
    [Header("강화 비용 (큰별)")]
    [SerializeField] private int[] tierCosts = { 12, 15 };

    [Header("공격력 — Q/W 공통 기본값 + 티어당 증가량")]
    [SerializeField] private int baseDamage1 = 8;  // Q 기본
    [SerializeField] private int baseDamage2 = 13; // W 기본
    [SerializeField] private int damagePerTier = 2; // 강화 1단계당 증가량 (Q/W 공통)

    public int MaxTier => tierCosts.Length;

    /// <summary>
    /// 지정한 tier에서 다음 단계로 강화하는 데 필요한 큰별 개수
    /// 이미 만렙(tier >= MaxTier)이면 -1
    /// </summary>
    public int GetCost(int currentTier) =>
        currentTier >= 0 && currentTier < MaxTier ? tierCosts[currentTier] : -1;

    public int GetDamage1(int tier) => baseDamage1 + damagePerTier * tier;
    public int GetDamage2(int tier) => baseDamage2 + damagePerTier * tier;
}
