using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class StageSelectButton : MonoBehaviour
{
    // 버튼
    [SerializeField] private int stageId;
    [SerializeField] private Button button;
    [SerializeField] private UnityEngine.UI.Image UIStageImage;
    [SerializeField] private Sprite lockedSprite;  
    [SerializeField] private Sprite unlockedSprite;    
    [SerializeField] private Sprite  clearedSprite;
    // 버튼 위의 클리어 상황
    [SerializeField] private UnityEngine.UI.Image UIStarImage;
    [SerializeField] private Sprite starSprite1;    
    [SerializeField] private Sprite starSprite2;
    [SerializeField] private Sprite starSprite3;

    [SerializeField] private Game.Stage.StageManager stageManager;
    public int StageId => stageId;

    public void Start()
    {
        StageProgress.Load(); // 방어 코드

        bool unlocked = StageProgress.IsStageUnlocked(stageId);
        bool cleared = false;

        if(unlocked)
            cleared = StageProgress.GetStarRank(stageId) > 0;

        button.interactable = unlocked;

        UpdateStageUI(unlocked, cleared);
        UpdateStarUI();
    }
    public void OnStageButtonClicked()
    {
        if (!StageProgress.IsStageUnlocked(stageId))
        {
            return;
        }
        StageProgress.SelectStage(stageId);
        SceneManager.LoadScene("Game");
    }
    private void UpdateStageUI(bool unlocked, bool cleared)
    {
        if (UIStageImage == null)
            return;

        if (!unlocked) // 잠긴 스테이지
        {
            UIStageImage.sprite = lockedSprite;
        }
        else if(cleared) // 클리어한 스테이지
        {
            UIStageImage.sprite = clearedSprite;
        }
        else // 해금된 스테이지
        {
            UIStageImage.sprite = unlockedSprite;
        }
    }
    private void UpdateStarUI()
    {
        int starCount = StageProgress.GetStarRank(stageId);

        if (UIStarImage == null)
            return;
        // 클리어한 적 없는 경우
        if (starCount <= 0) 
        {
            UIStarImage.gameObject.SetActive(false);
            return;
        }
        // 클리어한 경우
        UIStarImage.gameObject.SetActive(true);

        switch (starCount)
        {
            case 1:
                UIStarImage.sprite = starSprite1;
                break;
            case 2:
                UIStarImage.sprite = starSprite2;
                break;
            case 3:
                UIStarImage.sprite = starSprite3;
                break;
        }
    }

}
