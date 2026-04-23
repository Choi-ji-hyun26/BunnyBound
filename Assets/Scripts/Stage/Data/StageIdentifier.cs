using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageIdentifier : MonoBehaviour
{
    [Header("Stage Info")]
    [SerializeField] private int stageId;
    public int totalStarCount;
    public int StageId => stageId;
}