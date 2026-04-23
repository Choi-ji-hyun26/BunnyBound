using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageData
{
    public int StageId;

    public int BestScore;
    public int StarRank;

    public StageData(int stageId)
    {
        StageId = stageId;
        BestScore = 0;
        StarRank = 0;
    }
}
