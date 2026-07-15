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

    // ───────────────────────────────────
    // BuildSaveSnapshot — 스냅샷 보호 대상 조기 커밋 방지 (회귀 테스트)
    //
    // 배경: 스테이지 진행 중(스냅샷이 활성 상태) WeaponUpgradeManager.TryUpgrade()가
    // SaveImmediate()를 호출하면, 아직 커밋되지 않은 스킬 해금/상자/하트/힌트까지
    // 디스크에 조기 커밋되어 버리던 버그에 대한 회귀 방지
    // ───────────────────────────────────

    [Test]
    public void BuildSaveSnapshot_NoActiveSnapshot_ReturnsLiveDataUnchanged()
    {
        GameProgress.UnlockSkill(2);

        var saveData = GameProgress.BuildSaveSnapshot();

        Assert.IsTrue(saveData.player.unlockedSkills[1]);
    }

    [Test]
    public void BuildSaveSnapshot_ActiveSnapshot_RevertsProtectedFieldsToSnapshot()
    {
        GameProgress.TakeSnapshot(); // 이 시점에는 W 스킬 미해금, maxHearts 기본값(3) 상태

        GameProgress.UnlockSkill(2);   // 스테이지 중 미커밋 해금
        GameProgress.CollectChest(101);
        GameProgress.SaveMaxHearts(5);

        var saveData = GameProgress.BuildSaveSnapshot();

        Assert.IsFalse(saveData.player.unlockedSkills[1], "저장본은 스냅샷(미해금) 상태여야 함");
        Assert.IsFalse(saveData.player.collectedChestIds.Contains(101));
        Assert.AreEqual(3, saveData.player.maxHearts);
    }

    [Test]
    public void BuildSaveSnapshot_ActiveSnapshot_KeepsWeaponUpgradeFieldsAtLiveValue()
    {
        GameProgress.TakeSnapshot();

        GameProgress.UpdateStageResult(1, 100, 3); // 스타 적립은 스냅샷 보호 대상이 아니므로 정상 적립되어야 함
        GameProgress.SetWeaponUpgradeTier(1);

        var saveData = GameProgress.BuildSaveSnapshot();

        Assert.AreEqual(1, saveData.player.weaponUpgradeTier);
        Assert.AreEqual(3, saveData.player.spendableStars);
    }

    [Test]
    public void BuildSaveSnapshot_ActiveSnapshot_DoesNotMutateLiveState()
    {
        GameProgress.TakeSnapshot();
        GameProgress.UnlockSkill(2);

        GameProgress.BuildSaveSnapshot(); // 저장용 사본만 만들 뿐, 현재 세션의 라이브 상태는 유지되어야 함

        Assert.IsTrue(GameProgress.IsSkillUnlocked(2));
    }

    [Test]
    public void BuildSaveSnapshot_AfterCommitSnapshot_ReturnsLiveDataUnchanged()
    {
        GameProgress.TakeSnapshot();
        GameProgress.UnlockSkill(2);
        GameProgress.CommitSnapshot(); // 클리어로 확정 — 스냅샷 폐기

        var saveData = GameProgress.BuildSaveSnapshot();

        Assert.IsTrue(saveData.player.unlockedSkills[1], "CommitSnapshot 이후엔 라이브 상태 그대로 저장되어야 함");
    }
}
