using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingController : MonoBehaviour
{
    [SerializeField] private GameObject normalEndingUI;
    [SerializeField] private GameObject trueEndingUI;
    void Start()
    {
        GameProgress.Load(); // 안전장치

        bool isTrueEnding = GameProgress.IsAllStagesPerfect();

        normalEndingUI.SetActive(!isTrueEnding);
        trueEndingUI.SetActive(isTrueEnding);
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
