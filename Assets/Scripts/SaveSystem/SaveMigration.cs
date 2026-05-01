/// <summary>
/// 저장 파일 버전 마이그레이션
///
/// [버전 히스토리]
/// v0 → v1: StarRank 음수 보정
/// v1 → v2: 파일명 변경(stage_progress.json → game_progress.json)으로
///           v1 파일을 읽는 경우가 없어 마이그레이션 불필요. v2부터 새 시작.
/// </summary>
public static class SaveMigration
{
    public static SaveFile<GameProgressData> Migrate(SaveFile<GameProgressData> oldSave)
    {
        int version = oldSave.version;
        var data = oldSave.data;

        while (version < SaveVersion.CURRENT)
        {
            switch (version)
            {
                case 0:
                    data = Migrate_0_To_1(data);
                    break;
                // v1 → v2: 파일명 변경으로 v1 파일 접근 불가 → 마이그레이션 없음
            }
            version++;
        }
        return new SaveFile<GameProgressData>(SaveVersion.CURRENT, data);
    }

    // v0 → v1: StarRank 음수 보정
    static GameProgressData Migrate_0_To_1(GameProgressData old)
    {
        foreach (var s in old.stages)
        {
            if (s.StarRank < 0)
                s.StarRank = 0;
        }
        return old;
    }
}
