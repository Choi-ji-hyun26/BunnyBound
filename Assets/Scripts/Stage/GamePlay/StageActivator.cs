using System.Collections;
using System.Collections.Generic;
using Game.Stage;
using UnityEngine;

public class StageActivator : MonoBehaviour
{
    void Start()
    {
        int selectedStageId = StageProgress.CurrentStageId;
        if (selectedStageId <= 0)
        {
            Debug.LogError("StageActivator: invalid StageId");
            return;
        }

        StageIdentifier[] stages = FindObjectsOfType<StageIdentifier>(true); // true : 비활성 오브젝트 포함
        foreach(var stage in stages)
        {
            bool isActive = stage.StageId == selectedStageId; // true isActive만 활성화 나머지는 비활성화
            stage.gameObject.SetActive(isActive); 
        }
    }
}
