using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    [SerializeField] private GameObject SettingMenu;
    public void ViewSettingMenu() // public : 유니티 UI BUTTON 연결
    {
        SettingMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    public void Resume() // public : 유니티 UI BUTTON 연결
    {
        SettingMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Exit() // public : 유니티 UI BUTTON 연결
    {
        GameManager.Instance.Exit();
    }
}
