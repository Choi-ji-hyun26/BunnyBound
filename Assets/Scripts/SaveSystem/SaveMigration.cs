/// <summary>
/// 저장 파일 버전 마이그레이션
///
/// [버전 히스토리]
/// v0 → v1: StarRank 음수 보정
/// v1 → v2: 파일명 변경(stage_progress.json → game_progress.json)으로
///           v1 파일을 읽는 경우가 없어 마이그레이션 불필요, v2부터 새 시작
/// v2 → v3: PlayerProgressData에 collectedHintIds 필드 추가
/// v3 → v4: PlayerProgressData에 weaponUpgradeTier, spendableStars 필드 추가
///           기존 세이브는 스테이지별 StarRank 합산을 spendableStars 초기값으로 소급 적립
/// </summary>
public static class SaveMigration
{
    public static SaveFile<GameProgressData> Migrate(SaveFile<GameProgressData> oldSave)
    {
        try
        {
            int version = oldSave.version;
            var data = oldSave.data;

            // 비정상적인 버전 값(음수 또는 CURRENT를 초과) 방어
            // 손상된 세이브로 간주하고 catch 블록의 폴백 로직을 그대로 재사용
            if (version < 0 || version > SaveVersion.CURRENT)
                throw new System.Exception($"Invalid save version: {version}");

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
                    case 3:
                        data = Migrate_3_To_4(data);
                        break;
                }
                version++;
            }
            return new SaveFile<GameProgressData>(SaveVersion.CURRENT, data);
        }
        catch (System.Exception e)
        {
            // 마이그레이션 도중 손상된 데이터로 인한 예외 발생 시
            // 크래시 대신 새 진행 데이터로 안전하게 폴백
            UnityEngine.Debug.LogError($"Save migration failed (corrupted save assumed): {e}");
            return new SaveFile<GameProgressData>(SaveVersion.CURRENT, new GameProgressData());
        }
    }

    // v0 → v1: StarRank 음수 보정
    static GameProgressData Migrate_0_To_1(GameProgressData old)
    {
        // JsonUtility가 손상된 JSON을 필드 null 상태로 역직렬화할 수 있어 방어
        if (old.stages == null)
            return old;

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
        // old.player 자체가 null로 역직렬화된 손상 케이스 방어
        if (old.player == null)
            return old;

        if (old.player.collectedHintIds == null)
            old.player.collectedHintIds = new System.Collections.Generic.List<int>();
        return old;
    }

    // v3 → v4: weaponUpgradeTier, spendableStars 필드 추가
    // int 필드는 JsonUtility가 누락 시 기본값 0으로 채우므로 별도 null 방어는 불필요하나,
    // 이미 별을 모아둔 기존 세이브가 강화 잔액 0에서 시작하지 않도록
    // 스테이지별 최고 StarRank 합산을 spendableStars 초기값으로 소급 적립한다.
    static GameProgressData Migrate_3_To_4(GameProgressData old)
    {
        // old.player 자체가 null로 역직렬화된 손상 케이스 방어
        if (old.player == null)
            return old;

        int backfill = 0;
        if (old.stages != null)
        {
            foreach (var s in old.stages)
                backfill += s.StarRank;
        }
        old.player.spendableStars = backfill;
        return old;
    }
}
