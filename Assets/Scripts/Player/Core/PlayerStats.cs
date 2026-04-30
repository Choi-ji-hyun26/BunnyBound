using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어 HP 및 점수 관리
///
/// [HP 설계 — 젤다 방식]
/// - 토끼/검사 HP 통합: 캐릭터 타입 무관하게 하트 1개 감소
/// - 시작 하트: 3개 / 최대 하트: 6개
/// - 피격 시 오른쪽 하트부터 빈 하트로 교체
/// - HP 아이템(당근) 획득 시 최대 하트 +1 (빈 하트 슬롯 활성화)
///
/// [UI 구조]
/// Canvas → HPContainer (HorizontalLayoutGroup)
///   → HeartSlot_0 ~ HeartSlot_5 (Image 컴포넌트)
/// heartSlots 배열에 순서대로 연결
/// fullHeartSprite  = Tilesheet_1 (꽉 찬 하트)
/// emptyHeartSprite = Tilesheet_2 (빈 하트)
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    // ───────────────────────────────────────────
    // HP
    // ───────────────────────────────────────────
    [Header("HP Settings")]
    [SerializeField] private int startHearts = 3;       // 시작 하트 수
    [SerializeField] private int maxHearts = 6;         // 최대 하트 수 (슬롯 수와 일치)

    public int CurrentHearts { get; private set; }      // 현재 하트 수
    public int MaxActiveHearts { get; private set; }    // 현재 최대 하트 수 (아이템으로 증가)

    [Header("HP UI")]
    [SerializeField] private Image[] heartSlots;        // Inspector에서 HeartSlot_0~5 연결
    [SerializeField] private Sprite fullHeartSprite;    // Tilesheet_2
    [SerializeField] private Sprite emptyHeartSprite;   // Tilesheet_1

    // ───────────────────────────────────────────
    // 점수
    // ───────────────────────────────────────────
    public int stagePoint;
    [SerializeField] private TextMeshProUGUI UIPoint;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogWarning("씬에 두개 이상의 스탯 매니저가 존재합니다!");
            Destroy(gameObject);
            return;
        }

        MaxActiveHearts = startHearts;
        CurrentHearts = startHearts;
        RefreshHPUI();
    }

    private void Update()
    {
        if (UIPoint != null)
            UIPoint.text = stagePoint.ToString();
    }

    // ───────────────────────────────────────────
    // HP 감소 — 캐릭터 타입 무관 (토끼/검사 통합)
    // ───────────────────────────────────────────
    public void HealthDown()
    {
        CurrentHearts--;
        CurrentHearts = Mathf.Max(0, CurrentHearts);
        RefreshHPUI();

        if (CurrentHearts <= 0)
        {
            GetComponentInParent<PlayerDeathHandler>()?.OnDie();
            GameManager.Instance.ViewBtn();
        }
    }

    // ───────────────────────────────────────────
    // HP 회복 — 현재 하트 1개 회복 (최대 하트 내에서)
    // ───────────────────────────────────────────
    public void HealthUp()
    {
        if (CurrentHearts < MaxActiveHearts)
        {
            CurrentHearts++;
            RefreshHPUI();
        }
    }

    // ───────────────────────────────────────────
    // 최대 하트 증가 — HP 아이템 획득 시
    // 슬롯이 남아있을 때만 증가, 증가 후 새 하트는 꽉 찬 상태로 추가
    // ───────────────────────────────────────────
    public void IncreaseMaxHearts()
    {
        if (MaxActiveHearts >= maxHearts)
        {
            Debug.Log("[PlayerStats] 최대 하트 수에 도달했습니다.");
            return;
        }

        MaxActiveHearts++;
        CurrentHearts++;    // 새 하트는 꽉 찬 상태로 추가
        CurrentHearts = Mathf.Min(CurrentHearts, MaxActiveHearts);
        RefreshHPUI();
    }

    // ───────────────────────────────────────────
    // HP UI 갱신
    // MaxActiveHearts까지 꽉찬/빈 하트 표시
    // MaxActiveHearts 초과 슬롯은 비활성화
    // ───────────────────────────────────────────
    public void RefreshHPUI()
    {
        for (int i = 0; i < heartSlots.Length; i++)
        {
            if (heartSlots[i] == null) continue;

            if (i < MaxActiveHearts)
            {
                heartSlots[i].gameObject.SetActive(true);
                heartSlots[i].sprite = i < CurrentHearts ? fullHeartSprite : emptyHeartSprite;
            }
            else
            {
                // 아직 해금되지 않은 슬롯 — 비활성화
                heartSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // ───────────────────────────────────────────
    // 스테이지 전환 시 리셋
    // ───────────────────────────────────────────
    public void ResetForNextStage()
    {
        stagePoint = 0;

        // HP는 MaxActiveHearts 유지, 현재 하트만 풀 회복
        CurrentHearts = MaxActiveHearts;
        RefreshHPUI();

        PlayerTransformHandler transformHandler = FindObjectOfType<PlayerTransformHandler>();
        if (transformHandler != null)
            transformHandler.ResetToRabbit();
    }
}
