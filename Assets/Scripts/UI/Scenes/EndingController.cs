using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 씬 컨트롤러
/// - 퍼즐 클리어 시 진입, trueEndingUI 표시
/// - 챕터 2 구현 시 normalEndingUI 복구 및 씬 분리 예정
/// </summary>
public class EndingController : MonoBehaviour
{
    [SerializeField] private GameObject trueEndingUI;

    void Start()
    {
        GameProgress.Load();
        trueEndingUI.SetActive(true);
    }

    public void OnRestartButtonClick()
    {
        StartCoroutine(LoadWithFade("Title"));
    }

    private IEnumerator LoadWithFade(string sceneName)
    {
        if (FadeController.Instance != null)
            yield return StartCoroutine(FadeController.Instance.FadeOut());
        SceneManager.LoadScene(sceneName);
    }
}
