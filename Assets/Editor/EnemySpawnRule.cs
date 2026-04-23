using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/SpawnByDifficultyRule")]
public class EnemySpawnRule : ScriptableObject
{
    public List<EnemyDifficultyRange> enemyRanges;

    public GameObject Pick(float value)
    {
        foreach (var range in enemyRanges)
        {
            if (value >= range.minDifficulty && value < range.maxDifficulty)
                return range.enemyPrefab;
        }
        return null;
    }
}

