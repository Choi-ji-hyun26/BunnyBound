using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;
using TMPro;
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    public int stagePoint; // public : StarItem.cs, EndingController.cs, GameManager.cs

    public int health = 3; // public : GameManager.cs

    [SerializeField] private UnityEngine.UI.Image UIHealth;
    [SerializeField] private Sprite hpSprite3;
    [SerializeField] private Sprite hpSprite2;
    [SerializeField] private Sprite hpSprite1;
    [SerializeField] private Sprite hpSprite0;

    [SerializeField] private TextMeshProUGUI UIPoint;
    private void Awake()
    {
        //싱글톤 초기화
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Debug.LogWarning("씬에 두개 이상의 스탯 매니저가 존재합니다!");
            Destroy(gameObject); // 이미 존재하면 중복 방지
        }
    }

    private void Update()
    {
        if (UIPoint != null) //UI Point가 사용되는 씬에서만 실행되도록
            UIPoint.text = stagePoint.ToString();
    }

    public void HealthUp() // public : CarrotItem.cs
    {
        if (health < 3)
            health++;
        if (health == 2)
            UIHealth.sprite = hpSprite2;
        else
            UIHealth.sprite = hpSprite3;
    }
    public void HealthDown() // public : PlayerDeathHandler.cs, GameManager.cs
    {
        health--;
        if (health == 2)
        {
            UIHealth.sprite = hpSprite2;
        }
        else if (health == 1)
        {
            UIHealth.sprite = hpSprite1;
        }
        else if (health == 0)
        {
            UIHealth.sprite = hpSprite0;
            gameObject.GetComponentInParent<PlayerDeathHandler>().OnDie();
            GameManager.Instance.ViewBtn();
        }
    }
    public void ResetForNextStage()
    {
        health = 3;
        stagePoint = 0;
        UIHealth.sprite = hpSprite3;
    }
}