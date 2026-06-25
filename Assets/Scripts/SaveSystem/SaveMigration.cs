/// <summary>
/// 저장 파일 버전 마이그레이션
///
/// [버전 히스토리]
/// v0 → v1: StarRank 음수 보정
/// v1 → v2: 파일명 변경(stage_progress.json → game_progress.json)으로
///           v1 파일을 읽는 경우가 없어 마이그레이션 불필요, v2부터 새 시작
/// v2 → v3: PlayerProgressData에 collectedHintIds 필드 추가
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
                case 2:
                    data = Migrate_2_To_3(data);
                    break;
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

    // v2 → v3: collectedHintIds 필드 추가
    // JsonUtility는 누락 필드를 자동으로 기본값으로 채우지만
    // List<int>는 null로 역직렬화될 수 있어 명시적으로 초기화
    static GameProgressData Migrate_2_To_3(GameProgressData old)
    {
        if (old.player.collectedHintIds == null)
            old.player.collectedHintIds = new System.Collections.Generic.List<int>();
        return old;
    }
}
