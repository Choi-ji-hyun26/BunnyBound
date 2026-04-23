using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public static class StageProgress
{
    public static StageProgressData data;
    private static Dictionary<int, StageData> stageMap = new();
    private static string SavePath => Path.Combine(Application.persistentDataPath, "stage_progress.json");
    public static int CurrentStageId {get; private set;} = 1;

    public static void Load() // Load == Read
    {
        if(data != null)
            return;

        Application.quitting -= Flush; // 중복 등록 방지
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
        
        BuildCache(); // 딕셔너리 생성

        Application.quitting += Flush; // 종료 시 자동 저장 등록
        Application.focusChanged += OnFocusChanged; // 모바일 홈 화면 나갈 시 자동 저장 등록
    }

    private static void OnFocusChanged(bool hasFocus)
    {
        if(!hasFocus)
            Flush(); // 모바일에서 유저가 나갈 때 즉시 저장
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

    private static void Flush() // 실제 파일 쓰기
    {
        SaveManager.Flush(SavePath, data, SaveVersion.CURRENT);
    }

    public static void SelectStage(int stageId)
    {
        CurrentStageId = stageId;
    }

    public static bool IsStageCleared(int stageId)
    {
        if(stageMap.TryGetValue(stageId, out var data))
        {
            return data.StarRank > 0;
        }
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
        data.stages.Add(stageData); // 리스트에도 추가

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

        // best score
        if (score > stageData.BestScore)
        {
            stageData.BestScore = score;
            dirty = true;
        }
        if(starRank > stageData.StarRank)
        {
            stageData.StarRank = starRank; // 별은 내려가지 않음
            dirty = true;
        }

        if (dirty)
            SaveDeferred();
    }

    // 노말/진 엔딩 판단
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
