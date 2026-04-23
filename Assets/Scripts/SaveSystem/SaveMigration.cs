
public static class SaveMigration
{
    public static SaveFile<StageProgressData> Migrate(SaveFile<StageProgressData> oldSave)
    {
        int version = oldSave.version;
        var data = oldSave.data;

        while(version < SaveVersion.CURRENT)
        {
            switch (version)
            {
                case 0:
                    data = Migrate_0_To_1(data);
                    break;
            }
            version++;
        }
        return new SaveFile<StageProgressData>(SaveVersion.CURRENT, data);
    }
    static StageProgressData Migrate_0_To_1(StageProgressData old)
    {
        // 새 필드 추가 대응
        foreach(var s in old.stages)
        {
            if(s.StarRank < 0)
                s.StarRank = 0;
        }
        return old;
    }
}
