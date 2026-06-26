using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 버튼 — 스테이지 선택 씬에 배치
///
/// [동작]
/// - finalStageId 클리어 시 버튼 활성화
/// - 클릭 시 puzzleStageId 스테이지로 진입
///
/// [Inspector 설정]
/// - finalStageId : 버튼 해금 조건 스테이지 ID (기본값 9)
/// - puzzleStageId : 진입할 퍼즐 스테이지 ID (기본값 10)
/// </summary>
public class EndingButton : MonoBehaviour
{
    [SerializeField] private int finalStageId = 9;
    [SerializeField] private int puzzleStageId = 10;

    private void Start()
    {
        GameProgress.Load();
        bool isUnlocked = GameProgress.IsStageCleared(finalStageId);
        gameObject.SetActive(isUnlocked);
    }

    public void OnEndingButtonClicked()
    {
        GameProgress.SelectStage(puzzleStageId);
        StartCoroutine(LoadWithFade("Game"));
    }

    private IEnumerator LoadWithFade(string sceneName)
    {
        if (FadeController.Instance != null)
            yield return StartCoroutine(FadeController.Instance.FadeOut());
        SceneManager.LoadScene(sceneName);
    }
}
