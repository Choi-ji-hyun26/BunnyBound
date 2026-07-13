using NUnit.Framework;

/// <summary>
/// GameProgress 무기 강화 API Edit Mode 테스트
///
/// 배경: GameProgress는 static class라 테스트 간 상태가 공유됨
/// 각 테스트 [SetUp]에서 GameProgress.ResetForTest()로 격리 보장
/// </summary>
public class GameProgressWeaponUpgradeTests
{
    [SetUp]
    public void SetUp()
    {
        GameProgress.ResetForTest();
    }

    // ─────────────────────────────────────────
    // GetWeaponUpgradeTier / SetWeaponUpgradeTier
    // ─────────────────────────────────────────

    [Test]
    public void GetWeaponUpgradeTier_DefaultIsZero()
    {
        Assert.AreEqual(0, GameProgress.GetWeaponUpgradeTier());
    }

    [Test]
    public void SetWeaponUpgradeTier_UpdatesTier()
    {
        GameProgress.SetWeaponUpgradeTier(1);
        Assert.AreEqual(1, GameProgress.GetWeaponUpgradeTier());
    }

    // ─────────────────────────────────────────
    // GetSpendableStars / TrySpendStars
    // ─────────────────────────────────────────

    [Test]
    public void GetSpendableStars_DefaultIsZero()
    {
        Assert.AreEqual(0, GameProgress.GetSpendableStars());
    }

    [Test]
    public void TrySpendStars_SufficientBalance_SucceedsAndDeducts()
    {
        GameProgress.UpdateStageResult(1, 100, 3); // spendableStars = 3

        bool result = GameProgress.TrySpendStars(3);

        Assert.IsTrue(result);
        Assert.AreEqual(0, GameProgress.GetSpendableStars());
    }

    [Test]
    public void TrySpendStars_InsufficientBalance_FailsAndDoesNotDeduct()
    {
        GameProgress.UpdateStageResult(1, 100, 2); // spendableStars = 2

        bool result = GameProgress.TrySpendStars(3);

        Assert.IsFalse(result);
        Assert.AreEqual(2, GameProgress.GetSpendableStars());
    }

    // ─────────────────────────────────────────
    // UpdateStageResult — delta 적립 (반복 클리어 중복 방지)
    // ─────────────────────────────────────────

    [Test]
    public void UpdateStageResult_FirstClear_AccruesFullStarRank()
    {
        GameProgress.UpdateStageResult(1, 100, 2);

        Assert.AreEqual(2, GameProgress.GetSpendableStars());
    }

    [Test]
    public void UpdateStageResult_ImprovedRank_AccruesOnlyDelta()
    {
        GameProgress.UpdateStageResult(1, 100, 1); // spendableStars = 1
        GameProgress.UpdateStageResult(1, 100, 3); // 1 -> 3, delta = 2 적립 (총 3)

        Assert.AreEqual(3, GameProgress.GetSpendableStars());
    }

    [Test]
    public void UpdateStageResult_RepeatedClearSameRank_DoesNotDuplicateAccrual()
    {
        GameProgress.UpdateStageResult(1, 100, 3); // spendableStars = 3

        // 같은 랭크로 반복 클리어 — 별 랭크가 최고 기록을 넘지 않으므로 추가 적립 없음
        GameProgress.UpdateStageResult(1, 50, 3);
        GameProgress.UpdateStageResult(1, 80, 3);

        Assert.AreEqual(3, GameProgress.GetSpendableStars());
    }

    [Test]
    public void UpdateStageResult_MultipleStages_AccruesSumOfDeltas()
    {
        GameProgress.UpdateStageResult(1, 100, 3);
        GameProgress.UpdateStageResult(2, 100, 2);
        GameProgress.UpdateStageResult(3, 100, 1);

        Assert.AreEqual(6, GameProgress.GetSpendableStars());
    }
}
