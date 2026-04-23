using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
        StageProgress.Load();
    }
    public void LoadGame() // public : 유니티 UI BUTTON 연결
    {
        SceneManager.LoadScene("StageSelect");
    }
}
