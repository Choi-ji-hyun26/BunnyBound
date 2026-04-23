using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageClear : MonoBehaviour
{
    private bool isCleared;
    private void OnEnable()
    {
        isCleared = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCleared || !collision.CompareTag("Player"))
            return;

        isCleared = true;
        SoundManager.Instance.PlaySound("FINISH");
        GameManager.Instance.OnStageCleared();
    }
}
