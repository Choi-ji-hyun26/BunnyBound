using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;

/// <summary>
/// SaveMigration Edit Mode 테스트
///
/// 배경: JsonUtility는 손상된 JSON을 예외 없이 필드가 null인 객체로
/// 역직렬화하는 경우가 있어, Migrate() 도중 NullReferenceException으로 이어질 수 있었음
/// 이를 방지하기 위한 try-catch 폴백 및 null 가드, 버전 방어 로직을 검증
/// </summary>
public class SaveMigrationTests
{
    // ─────────────────────────────────────────
    // null 데이터 방어
    // ─────────────────────────────────────────

    [Test]
    public void Migrate_NullStages_DoesNotThrow()
    {
        var oldSave = new SaveFile<GameProgressData>(0, new GameProgressData
        {
            stages = null,
            player = new PlayerProgressData()
        });

        Assert.DoesNotThrow(() => SaveMigration.Migrate(oldSave));
    }

    [Test]
    public void Migrate_NullPlayer_DoesNotThrow()
    {
        var oldSave = new SaveFile<GameProgressData>(2, new GameProgressData
        {
            stages = new List<StageData>(),
            player = null
        });

        Assert.DoesNotThrow(() => SaveMigration.Migrate(oldSave));
    }

    // ─────────────────────────────────────────
    // 버전 방어
    // ─────────────────────────────────────────

    [Test]
    public void Migrate_NegativeVersion_FallsBackToNewData()
    {
        // Migrate() 내부에서 방어 로직이 의도적으로 Debug.LogError를 남기므로
        // Test Runner가 이를 미확인 에러로 간주해 실패 처리하지 않도록 미리 예상 로그로 등록
        LogAssert.Expect(UnityEngine.LogType.Error, new Regex("Save migration failed.*"));

        var oldSave = new SaveFile<GameProgressData>(-1, new GameProgressData());

        var result = SaveMigration.Migrate(oldSave);

        Assert.AreEqual(SaveVersion.CURRENT, result.version);
        Assert.IsNotNull(result.data);
    }

    [Test]
    public void Migrate_VersionExceedsCurrent_FallsBackToNewData()
    {
        LogAssert.Expect(UnityEngine.LogType.Error, new Regex("Save migration failed.*"));

        var oldSave = new SaveFile<GameProgressData>(SaveVersion.CURRENT + 1, new GameProgressData());

        var result = SaveMigration.Migrate(oldSave);

        Assert.AreEqual(SaveVersion.CURRENT, result.version);
        Assert.IsNotNull(result.data);
    }

    // ─────────────────────────────────────────
    // 정상 마이그레이션 경로 (v0 → CURRENT)
    // ─────────────────────────────────────────

    [Test]
    public void Migrate_V0ToCurrent_CorrectsNegativeStarRank()
    {
        var stage = new StageData(1) { StarRank = -1 };
        var oldSave = new SaveFile<GameProgressData>(0, new GameProgressData
        {
            stages = new List<StageData> { stage },
            player = new PlayerProgressData()
        });

        var result = SaveMigration.Migrate(oldSave);

        Assert.AreEqual(0, result.data.stages[0].StarRank);
    }

    [Test]
    public void Migrate_V0ToCurrent_InitializesNullHintIds()
    {
        // JsonUtility가 역직렬화 시 List<int> 필드를 null로 채우는 상황을 재현
        var player = new PlayerProgressData { collectedHintIds = null };
        var oldSave = new SaveFile<GameProgressData>(0, new GameProgressData
        {
            stages = new List<StageData>(),
            player = player
        });

        var result = SaveMigration.Migrate(oldSave);

        Assert.IsNotNull(result.data.player.collectedHintIds);
    }

    // ─────────────────────────────────────────
    // 이미 최신 버전인 경우 (마이그레이션 불필요)
    // ─────────────────────────────────────────

    [Test]
    public void Migrate_AlreadyCurrentVersion_ReturnsDataUnchanged()
    {
        var data = new GameProgressData();
        var oldSave = new SaveFile<GameProgressData>(SaveVersion.CURRENT, data);

        var result = SaveMigration.Migrate(oldSave);

        Assert.AreEqual(SaveVersion.CURRENT, result.version);
        Assert.AreSame(data, result.data);
    }
}
