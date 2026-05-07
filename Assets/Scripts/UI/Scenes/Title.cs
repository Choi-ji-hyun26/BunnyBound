using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
        GameProgress.Load();
    }
    public void LoadGame()
    {
        StartCoroutine(LoadWithFade("StageSelect"));
    }

    private IEnumerator LoadWithFade(string sceneName)
    {
        if (FadeController.Instance != null)
            yield return StartCoroutine(FadeController.Instance.FadeOut());
        SceneManager.LoadScene(sceneName);
    }
}
