using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public static class StageProgress
{
    public static StageProgressData data;
    private static Dictionary<int, StageData> stageMap = new();
    private static string SavePath => Path.Combine(Application.persistentDataPath, "game_progress.json");

    public static void Load()
    {
        if(data != null)
            return;

        Application.quitting -= Flush;
        Application.focusChanged -= OnFocusChanged;
        
        var wrapper = SaveManager.Load<StageProgressData>(SavePath);
                
        if(wrapper == null)
        {
            data = new StageProgressData();
        }
        else
        {
            if(wrapper.version < SaveVersion.CURRENT)
                wrapper = SaveMigration.Migrate(wrapper);

            data = wrapper.data;
        }
        
        BuildCache();

        Application.quitting += Flush;
        Application.focusChanged += OnFocusChanged;
    }

    private static void OnFocusChanged(bool hasFocus)
    {
        if(!hasFocus)
            Flush();
    }

    private static void SaveDeferred()
    {
        SaveManager.MarkDirty();
    }

    public static void SaveImmediate()
    {
        SaveManager.MarkDirty();
        Flush();
    }

    private static void Flush()
    {
        SaveManager.Flush(SavePath, data, SaveVersion.CURRENT);
    }

    public static void SelectStage(int stageId)
    {
        CurrentStageId = stageId;
    }

    public static int CurrentStageId {get; private set;} = 1;

    public static bool IsStageCleared(int stageId)
    {
        if(stageMap.TryGetValue(stageId, out var data))
            return data.StarRank > 0;
        return false;
    }

    public static bool IsStageUnlocked(int stageId)
    {
        if(stageId == 1) return true;
        return IsStageCleared(stageId - 1);
    }

    public static StageData GetStageData(int stageId)
    {
        if(stageMap.TryGetValue(stageId, out var stageData))
            return stageData;
        stageData = new StageData(stageId);
        stageMap[stageId] = stageData;
        data.stages.Add(stageData);

        SaveDeferred();
        return stageData;
    }

    public static int GetBestScore(int stageId)
    {
        return stageMap.TryGetValue(stageId, out var data)
            ? data.BestScore : 0;
    }

    public static int GetStarRank(int stageId)
    {
        return stageMap.TryGetValue(stageId, out var data)
            ? data.StarRank : 0;
    }

    public static void UpdateStageResult(int stageId, int score, int starRank)
    {
        StageData stageData = GetStageData(stageId);
        
        bool dirty = false;

        if (score > stageData.BestScore)
        {
            stageData.BestScore = score;
            dirty = true;
        }
        if(starRank > stageData.StarRank)
        {
            stageData.StarRank = starRank;
            dirty = true;
        }

        if (dirty)
            SaveDeferred();
    }

    public static bool IsAllStagesPerfect()
    {
        if(data.stages == null || data.stages.Count == 0)
            return true;

        foreach (StageData stage in data.stages)
        {
            if (stage.StarRank < 3)
                return false;
        }
        return true;
    }

    private static void BuildCache()
    {
        stageMap.Clear();

        if(data.stages == null)
            data.stages = new List<StageData>();

        foreach(var s in data.stages)
            stageMap[s.StageId] = s;
    }
}
