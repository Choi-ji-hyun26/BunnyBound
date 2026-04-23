using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Stage
{
    public class StageManager : MonoBehaviour
    {
        public void EnterStage(int stageId)
        {
            if (!StageProgress.IsStageUnlocked(stageId))
                return;

            StageProgress.SelectStage(stageId);
            SceneManager.LoadScene("Game");
        }
    }
}