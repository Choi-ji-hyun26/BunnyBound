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

    public int health = 3; // public : GameManager.cs (토끼 HP 칸 수)

    // 검사 HP (0~100%)
    [HideInInspector] public float swordsmanHP = 100f;
    [SerializeField] private UnityEngine.UI.Slider swordsmanHPSlider; // 검사 HP UI (슬라이더)

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
        SetSwordsmanHP(100f);
    }

    // ───────────────────────────────────────────
    // 검사 HP 관련
    // ───────────────────────────────────────────

    /// <summary>
    /// 검사 HP 설정 및 UI 갱신
    /// </summary>
    public void SetSwordsmanHP(float value)
    {
        swordsmanHP = Mathf.Clamp(value, 0f, 100f);
        RefreshSwordsmanHPUI();

        if (swordsmanHP <= 0f)
        {
            gameObject.GetComponentInParent<PlayerDeathHandler>().OnDie();
            GameManager.Instance.ViewBtn();
        }
    }

    /// <summary>
    /// 검사 피격 시 HP 감소 (PlayerDamageHandler에서 호출)
    /// </summary>
    public void SwordsmanHealthDown(float amount = 18f)
    {
        SetSwordsmanHP(swordsmanHP - amount);
    }

    /// <summary>
    /// 검사 HP UI 갱신
    /// </summary>
    public void RefreshSwordsmanHPUI()
    {
        if (swordsmanHPSlider != null)
            swordsmanHPSlider.value = swordsmanHP / 100f;
    }

    /// <summary>
    /// 토끼 HP UI 갱신 (변신 복귀 시 호출)
    /// </summary>
    public void RefreshRabbitHPUI()
    {
        if (UIHealth == null) return;
        if (health >= 3)      UIHealth.sprite = hpSprite3;
        else if (health == 2) UIHealth.sprite = hpSprite2;
        else if (health == 1) UIHealth.sprite = hpSprite1;
        else                  UIHealth.sprite = hpSprite0;
    }
}