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
    public void OnRestartButtonClick() // public : 유니티 UI BUTTON 연결
    {
        SceneManager.LoadScene("Title");
    }
}
