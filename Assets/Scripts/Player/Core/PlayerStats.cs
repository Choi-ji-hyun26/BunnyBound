using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어 HP 및 점수 관리
///
/// [HP 설계]
/// - 내부 HP는 정수 (하트 1개 = HP_PER_HEART = 4)
/// - 토끼/검사 HP 통합: 캐릭터 타입 무관하게 동일한 데미지 처리
/// - 시작 하트: 3개 / 최대 하트: 6개
/// - MaxActiveHearts는 GameProgress에서 로드/저장
/// - HP Up 아이템: 최대 하트 +1 + 전체 회복
///
/// [슬롯별 렌더링 로직]
/// slotHP = CurrentHP - (i * HP_PER_HEART)
/// slotHP >= 4 → full / == 3 → 3/4 / == 2 → half / == 1 → quarter / <= 0 → empty
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    // ───────────────────────────────────────────
    // HP 상수
    // ───────────────────────────────────────────
    public const int HP_PER_HEART = 4;

    // ───────────────────────────────────────────
    // HP
    // ───────────────────────────────────────────
    [Header("HP Settings")]
    [SerializeField] private int startHearts = 3;
    [SerializeField] private int maxHearts = 6;     // 슬롯 수 상한선

    public int CurrentHP { get; private set; }
    public int MaxHP => MaxActiveHearts * HP_PER_HEART;
    public int MaxActiveHearts { get; private set; }

    [Header("HP UI")]
    [SerializeField] private Image[] heartSlots;
    [SerializeField] private Sprite fullHeartSprite;        
    [SerializeField] private Sprite threeQuarterSprite;     
    [SerializeField] private Sprite halfHeartSprite;        
    [SerializeField] private Sprite quarterHeartSprite;     
    [SerializeField] private Sprite emptyHeartSprite;       

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

        // GameProgress에서 최대 하트 수 로드
        GameProgress.Load();
        MaxActiveHearts = Mathf.Clamp(GameProgress.GetMaxHearts(), startHearts, maxHearts);
        CurrentHP = MaxHP;
        RefreshHPUI();
    }

    private void Update()
    {
        if (UIPoint != null)
            UIPoint.text = stagePoint.ToString();
    }

    // ───────────────────────────────────────────
    // 데미지 처리 — 적별 amount 전달
    // ───────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        RefreshHPUI();

        if (CurrentHP <= 0)
        {
            GetComponentInParent<PlayerDeathHandler>()?.OnDie();
            GameManager.Instance.ViewBtn();
        }
    }

    // ───────────────────────────────────────────
    // HP 회복 — 1/4하트(1) 단위 회복
    // ───────────────────────────────────────────
    public void HealthUp()
    {
        if (CurrentHP < MaxHP)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + HP_PER_HEART);
            RefreshHPUI();
        }
    }

    // ───────────────────────────────────────────
    // 전체 HP 회복
    // ───────────────────────────────────────────
    public void FullHeal()
    {
        CurrentHP = MaxHP;
        RefreshHPUI();
    }

    // ───────────────────────────────────────────
    // 최대 하트 증가 — HP Up 아이템 획득 시
    // 슬롯이 남아있을 때만 증가, GameProgress에 저장
    // ───────────────────────────────────────────
    public void IncreaseMaxHearts()
    {
        if (MaxActiveHearts >= maxHearts)
        {
            Debug.Log("[PlayerStats] 최대 하트 수에 도달했습니다.");
            return;
        }

        MaxActiveHearts++;
        CurrentHP = Mathf.Min(CurrentHP + HP_PER_HEART, MaxHP);

        // 저장
        GameProgress.SaveMaxHearts(MaxActiveHearts);
        RefreshHPUI();
    }

    // ───────────────────────────────────────────
    // HP UI 갱신 — 슬롯별 잔량으로 4단계 스프라이트 결정
    // ───────────────────────────────────────────
    public void RefreshHPUI()
    {
        for (int i = 0; i < heartSlots.Length; i++)
        {
            if (heartSlots[i] == null) continue;

            if (i < MaxActiveHearts)
            {
                heartSlots[i].gameObject.SetActive(true);
                int slotHP = CurrentHP - (i * HP_PER_HEART);
                heartSlots[i].sprite = GetHeartSprite(slotHP);
            }
            else
            {
                heartSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private Sprite GetHeartSprite(int slotHP)
    {
        if (slotHP >= 4) return fullHeartSprite;
        if (slotHP == 3) return threeQuarterSprite;
        if (slotHP == 2) return halfHeartSprite;
        if (slotHP == 1) return quarterHeartSprite;
        return emptyHeartSprite;
    }

    // ───────────────────────────────────────────
    // 스테이지 전환 시 리셋
    // MaxActiveHearts 유지 (저장된 값 그대로), 현재 HP만 풀 회복
    // ───────────────────────────────────────────
    public void ResetForNextStage()
    {
        stagePoint = 0;
        CurrentHP = MaxHP;
        RefreshHPUI();

        PlayerTransformHandler transformHandler = FindObjectOfType<PlayerTransformHandler>();
        if (transformHandler != null)
            transformHandler.ResetToRabbit();
    }
}
