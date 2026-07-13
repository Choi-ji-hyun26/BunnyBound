using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

/// <summary>
/// 게임 전체 진행 데이터 관리 (구 StageProgress)
/// - 스테이지 진행 데이터 (StageData)
/// - 플레이어 성장 데이터 (PlayerProgressData)
/// 저장 파일: game_progress.json
///
/// [저장 타이밍 — 스테이지 클리어 시 일괄 저장]
/// 모든 데이터(스킬 해금, 하트, 스테이지 결과)는 메모리에만 반영하다가
/// OnStageCleared()의 SaveImmediate() 호출 시 한꺼번에 파일에 저장.
///
/// [스냅샷 방식으로 롤백]
/// 스테이지 진입 시 player 데이터 스냅샷 저장.
/// 클리어 없이 이탈(BackToStageSelect, Restart) 시 스냅샷으로 복원.
/// → 클리어 전 획득한 스킬/하트는 이탈 시 메모리에서도 롤백됨.
/// </summary>
public static class GameProgress
{
    public static GameProgressData data;
    private static Dictionary<int, StageData> stageMap = new();
    private static string SavePath => Path.Combine(Application.persistentDataPath, "game_progress.json");

    public static int CurrentStageId { get; private set; } = 1;

    // 스테이지 진입 시점의 player 스냅샷
    private static PlayerProgressData playerSnapshot;

    // ───────────────────────────────────────────
    // 로드
    // ───────────────────────────────────────────
    public static void Load()
    {
        if (data != null)
            return;

        Application.quitting -= Flush;
        Application.focusChanged -= OnFocusChanged;

        var wrapper = SaveManager.Load<GameProgressData>(SavePath);

        if (wrapper == null)
        {
            data = new GameProgressData();
        }
        else
        {
            if (wrapper.version < SaveVersion.CURRENT)
                wrapper = SaveMigration.Migrate(wrapper);

            data = wrapper.data;
        }

        BuildCache();

        Application.quitting += Flush;
        Application.focusChanged += OnFocusChanged;
    }

    private static void OnFocusChanged(bool hasFocus)
    {
        if (!hasFocus)
            Flush();
    }

    // ───────────────────────────────────────────
    // 저장
    // ───────────────────────────────────────────
    private static void SaveDeferred() => SaveManager.MarkDirty();

    public static void SaveImmediate()
    {
        SaveManager.MarkDirty();
        Flush();
    }

    private static void Flush() => SaveManager.Flush(SavePath, data, SaveVersion.CURRENT);

    // ───────────────────────────────────────────
    // 스냅샷 — 스테이지 진입/이탈 시 사용
    // ───────────────────────────────────────────

    /// <summary>
    /// 스테이지 진입 시 호출 — player 데이터 스냅샷 저장
    /// 클리어 없이 이탈 시 RollbackPlayer()로 복원
    /// </summary>
    public static void TakeSnapshot()
    {
        if (data?.player == null) return;

        playerSnapshot = new PlayerProgressData
        {
            unlockedSkills = (bool[])data.player.unlockedSkills.Clone(),
            maxHearts = data.player.maxHearts,
            collectedChestIds = new System.Collections.Generic.List<int>(data.player.collectedChestIds),
            collectedHintIds = new System.Collections.Generic.List<int>(data.player.collectedHintIds)
        };
    }

    /// <summary>
    /// 클리어 없이 이탈 시 호출 — player 데이터를 스냅샷으로 복원
    /// 클리어 시에는 호출하지 않음 (SaveImmediate()로 확정)
    /// </summary>
    public static void RollbackPlayer()
    {
        if (playerSnapshot == null) return;

        data.player.unlockedSkills = (bool[])playerSnapshot.unlockedSkills.Clone();
        data.player.maxHearts = playerSnapshot.maxHearts;
        data.player.collectedChestIds = new List<int>(playerSnapshot.collectedChestIds);
        data.player.collectedHintIds = new List<int>(playerSnapshot.collectedHintIds);
        playerSnapshot = null;
    }

    /// <summary>
    /// 클리어 시 호출 — 스냅샷 폐기 (롤백 필요 없음)
    /// </summary>
    public static void CommitSnapshot()
    {
        playerSnapshot = null;
    }

    // ───────────────────────────────────────────
    // 스테이지 API
    // ───────────────────────────────────────────
    public static void SelectStage(int stageId) => CurrentStageId = stageId;

    public static bool IsStageCleared(int stageId)
    {
        if (stageMap.TryGetValue(stageId, out var d))
            return d.StarRank > 0;
        return false;
    }

    public static bool IsStageUnlocked(int stageId)
    {
        if (stageId == 1) return true;
        return IsStageCleared(stageId - 1);
    }

    public static StageData GetStageData(int stageId)
    {
        if (stageMap.TryGetValue(stageId, out var stageData))
            return stageData;

        stageData = new StageData(stageId);
        stageMap[stageId] = stageData;
        data.stages.Add(stageData);

        return stageData;
    }

    public static int GetBestScore(int stageId) =>
        stageMap.TryGetValue(stageId, out var d) ? d.BestScore : 0;

    public static int GetStarRank(int stageId) =>
        stageMap.TryGetValue(stageId, out var d) ? d.StarRank : 0;

    public static void UpdateStageResult(int stageId, int score, int starRank)
    {
        StageData stageData = GetStageData(stageId);
        bool dirty = false;

        if (score > stageData.BestScore)
        {
            stageData.BestScore = score;
            dirty = true;
        }
        if (starRank > stageData.StarRank)
        {
            // 증가분(delta)만큼만 강화 잔액에 적립 — 반복 클리어로 중복 적립되지 않도록
            int delta = starRank - stageData.StarRank;
            if (data.player != null)
                data.player.spendableStars += delta;

            stageData.StarRank = starRank;
            dirty = true;
        }

        if (dirty) SaveDeferred();
    }

    public static bool IsAllStagesPerfect()
    {
        if (data.stages == null || data.stages.Count == 0)
            return true;

        foreach (StageData stage in data.stages)
        {
            if (stage.StarRank < 3)
                return false;
        }
        return true;
    }

    // ───────────────────────────────────────────
    // 플레이어 성장 API
    // ───────────────────────────────────────────
    public static bool IsSkillUnlocked(int attackIndex)
    {
        if (data?.player?.unlockedSkills == null) return attackIndex == 1;
        if (attackIndex < 1 || attackIndex > data.player.unlockedSkills.Length) return false;
        return data.player.unlockedSkills[attackIndex - 1];
    }

    /// <summary>
    /// 스킬 해금 — 메모리에만 반영
    /// 클리어 시 SaveImmediate()로 저장, 이탈 시 RollbackPlayer()로 복원
    /// </summary>
    public static void UnlockSkill(int attackIndex)
    {
        if (data?.player?.unlockedSkills == null) return;
        if (attackIndex < 1 || attackIndex > data.player.unlockedSkills.Length) return;

        data.player.unlockedSkills[attackIndex - 1] = true;
    }

    public static int GetMaxHearts() => data?.player?.maxHearts ?? 3;

    /// <summary>
    /// 최대 하트 수 갱신 — 메모리에만 반영
    /// 클리어 시 SaveImmediate()로 저장, 이탈 시 RollbackPlayer()로 복원
    /// </summary>
    public static void SaveMaxHearts(int maxHearts)
    {
        if (data?.player == null) return;
        data.player.maxHearts = maxHearts;
    }

    // ───────────────────────────────────────────
    // 무기 강화 API
    // ───────────────────────────────────────────
    public static int GetWeaponUpgradeTier() => data?.player?.weaponUpgradeTier ?? 0;

    public static void SetWeaponUpgradeTier(int tier)
    {
        if (data?.player == null) return;
        data.player.weaponUpgradeTier = tier;
    }

    public static int GetSpendableStars() => data?.player?.spendableStars ?? 0;

    /// <summary>
    /// 큰별 소비 — 잔액이 부족하면 실패(false)
    /// </summary>
    public static bool TrySpendStars(int amount)
    {
        if (data?.player == null) return false;
        if (data.player.spendableStars < amount) return false;

        data.player.spendableStars -= amount;
        return true;
    }

    // ───────────────────────────────────────────
    // 1회용 보물상자 API
    // ───────────────────────────────────────────

    /// <summary>
    /// 해당 chestId의 상자를 이미 획득했는지 확인
    /// </summary>
    public static bool IsChestCollected(int chestId)
    {
        return data?.player?.collectedChestIds?.Contains(chestId) ?? false;
    }

    /// <summary>
    /// 1회용 상자 획득 기록 — 메모리에만 반영
    /// 클리어 시 SaveImmediate()로 저장, 이탈 시 RollbackPlayer()로 복원
    /// </summary>
    public static void CollectChest(int chestId)
    {
        if (data?.player == null) return;
        if (!data.player.collectedChestIds.Contains(chestId))
            data.player.collectedChestIds.Add(chestId);
    }

    // ───────────────────────────────────────────
    // 힌트 API
    // ───────────────────────────────────────────

    /// <summary>
    /// 해당 hintId의 힌트를 이미 획득했는지 확인
    /// </summary>
    public static bool IsHintCollected(int hintId)
    {
        return data?.player?.collectedHintIds?.Contains(hintId) ?? false;
    }

    /// <summary>
    /// 힌트 획득 기록 — 메모리에만 반영
    /// 클리어 시 SaveImmediate()로 저장, 이탈 시 RollbackPlayer()로 복원
    /// </summary>
    public static void CollectHint(int hintId)
    {
        if (data?.player == null) return;
        if (!data.player.collectedHintIds.Contains(hintId))
            data.player.collectedHintIds.Add(hintId);
    }

    /// <summary>
    /// 현재 획득한 힌트 ID 목록 반환
    /// HintSlotUI에서 슬롯 구성에 사용
    /// </summary>
    public static List<int> GetCollectedHintIds()
    {
        return data?.player?.collectedHintIds ?? new List<int>();
    }

    /// <summary>
    /// 챕터 클리어 시 힌트 목록 초기화
    /// SequenceGate에서 정답 확인 후 호출
    /// </summary>
    public static void ClearHints()
    {
        if (data?.player == null) return;
        data.player.collectedHintIds.Clear();
    }

    // ───────────────────────────────────────────
    // 테스트 지원
    // ───────────────────────────────────────────

    /// <summary>
    /// Edit Mode 테스트 전용 — static 상태(data, stageMap) 초기화
    /// GameProgress는 static class라 테스트 간 상태가 공유되므로
    /// 각 테스트의 [SetUp]에서 호출해 격리를 보장
    /// 프로덕션 코드에서는 호출하지 않음
    /// </summary>
    public static void ResetForTest(GameProgressData newData = null)
    {
        data = newData ?? new GameProgressData();
        BuildCache();
    }

    // ───────────────────────────────────────────
    // 내부
    // ───────────────────────────────────────────
    private static void BuildCache()
    {
        stageMap.Clear();

        if (data.stages == null)
            data.stages = new List<StageData>();

        foreach (var s in data.stages)
            stageMap[s.StageId] = s;
    }
}
