using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private int stageId;
    [SerializeField] private Button button;
    [SerializeField] private Image UIStageImage;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite clearedSprite;

    [SerializeField] private Image UIStarImage;
    [SerializeField] private Sprite starSprite1;
    [SerializeField] private Sprite starSprite2;
    [SerializeField] private Sprite starSprite3;

    [SerializeField] private Game.Stage.StageManager stageManager;
    public int StageId => stageId;

    public void Start()
    {
        GameProgress.Load(); // 방어 코드

        bool unlocked = GameProgress.IsStageUnlocked(stageId);
        bool cleared = unlocked && GameProgress.GetStarRank(stageId) > 0;

        button.interactable = unlocked;

        UpdateStageUI(unlocked, cleared);
        UpdateStarUI();
    }

    public void OnStageButtonClicked()
    {
        if (!GameProgress.IsStageUnlocked(stageId)) return;
        GameProgress.SelectStage(stageId);
        SceneManager.LoadScene("Game");
    }

    private void UpdateStageUI(bool unlocked, bool cleared)
    {
        if (UIStageImage == null) return;

        if (!unlocked)
            UIStageImage.sprite = lockedSprite;
        else if (cleared)
            UIStageImage.sprite = clearedSprite;
        else
            UIStageImage.sprite = unlockedSprite;
    }

    private void UpdateStarUI()
    {
        int starCount = GameProgress.GetStarRank(stageId);

        if (UIStarImage == null) return;

        if (starCount <= 0)
        {
            UIStarImage.gameObject.SetActive(false);
            return;
        }

        UIStarImage.gameObject.SetActive(true);
        UIStarImage.sprite = starCount switch
        {
            1 => starSprite1,
            2 => starSprite2,
            3 => starSprite3,
            _ => starSprite1
        };
    }
}
