using UnityEngine;

/// <summary>
/// 무기 강화 비용 테이블
/// - tierCosts[i]: (i+1)차 강화에 필요한 큰별(spendableStars) 개수
/// - 배열 길이가 곧 최대 강화 단계(MaxTier) — 별도 상수와 어긋날 위험 없음
/// - StageSelect/Game 씬의 WeaponUpgradeManager 프리팹이 이 에셋 하나를 공유 참조
///   (프리팹별로 값이 따로 존재해 어긋나는 것을 방지)
/// </summary>
[CreateAssetMenu(fileName = "WeaponUpgradeConfig", menuName = "BunnyBound/WeaponUpgradeConfig")]
public class WeaponUpgradeConfig : ScriptableObject
{
    [SerializeField] private int[] tierCosts = { 12, 15 };

    public int MaxTier => tierCosts.Length;

    /// <summary>
    /// 지정한 tier에서 다음 단계로 강화하는 데 필요한 큰별 개수
    /// 이미 만렙(tier >= MaxTier)이면 -1
    /// </summary>
    public int GetCost(int currentTier) =>
        currentTier >= 0 && currentTier < MaxTier ? tierCosts[currentTier] : -1;
}
