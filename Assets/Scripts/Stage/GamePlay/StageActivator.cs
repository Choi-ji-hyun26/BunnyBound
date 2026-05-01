using Game.Stage;
using UnityEngine;

public class StageActivator : MonoBehaviour
{
    void Start()
    {
        int selectedStageId = GameProgress.CurrentStageId;
        if (selectedStageId <= 0)
        {
            Debug.LogError("StageActivator: invalid StageId");
            return;
        }

        StageIdentifier[] stages = FindObjectsOfType<StageIdentifier>(true);
        foreach (var stage in stages)
        {
            stage.gameObject.SetActive(stage.StageId == selectedStageId);
        }
    }
}
