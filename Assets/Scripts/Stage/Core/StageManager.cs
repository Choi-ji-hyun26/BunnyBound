using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Stage
{
    public class StageManager : MonoBehaviour
    {
        public void EnterStage(int stageId)
        {
            if (!GameProgress.IsStageUnlocked(stageId))
                return;

            GameProgress.SelectStage(stageId);
            SceneManager.LoadScene("Game");
        }
    }
}
