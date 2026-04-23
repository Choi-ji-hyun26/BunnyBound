using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyDifficultyRange
{   
    public GameObject enemyPrefab;
    [Range(0f, 1f)] public float minDifficulty;
    [Range(0f, 1f)] public float maxDifficulty;
}
