using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Q, W 공격 / 쉴드 쿨타임 UI
/// 데스크탑 전용. 모바일에서는 skillSlotContainer를 비활성화합니다.
/// </summary>
public class CooldownUIController : MonoBehaviour
{
    [Header("Q 공격 슬롯")]
    [SerializeField] private Image qAttackOverlay;
    [SerializeField] private TextMeshProUGUI qAttackText;

    [Header("W 공격 슬롯")]
    [SerializeField] private GameObject wSlot;
    [SerializeField] private Image wAttackOverlay;
    [SerializeField] private TextMeshProUGUI wAttackText;

    [Header("쉴드 슬롯")]
    [SerializeField] private Image shieldOverlay;
    [SerializeField] private TextMeshProUGUI shieldText;

    [Header("참조")]
    [SerializeField] private PlayerSwordAttackHandler swordAttackHandler;
    [SerializeField] private PlayerShieldHandler shieldHandler;

    [Header("플랫폼 제어")]
    [SerializeField] private GameObject skillSlotContainer;

    private void Awake()
    {
        if (swordAttackHandler == null)
            Debug.LogError("[CooldownUIController] swordAttackHandler가 연결되지 않았습니다.");
        if (shieldHandler == null)
            Debug.LogError("[CooldownUIController] shieldHandler가 연결되지 않았습니다.");

        SetOverlay(qAttackOverlay, qAttackText, 0f, 0f);
        SetOverlay(wAttackOverlay, wAttackText, 0f, 0f);
        SetOverlay(shieldOverlay, shieldText, 0f, 0f);
    }

    private void Start()
    {
        // 모바일에서는 Skill Slot Container 비활성화 — 데스크탑 전용 UI
        if (Application.isMobilePlatform)
        {
            if (skillSlotContainer != null)
                skillSlotContainer.SetActive(false);
            return;
        }

        // W 슬롯 초기 상태 — 1회만 실행
        if (wSlot != null)
            wSlot.SetActive(SkillUnlockManager.Instance != null &&
                            SkillUnlockManager.Instance.IsUnlocked(2));
    }

    private void OnEnable()
    {
        if (SkillUnlockManager.Instance != null)
            SkillUnlockManager.Instance.OnSkillUnlocked += OnSkillUnlocked;
    }

    private void OnDisable()
    {
        if (SkillUnlockManager.Instance != null)
            SkillUnlockManager.Instance.OnSkillUnlocked -= OnSkillUnlocked;
    }

    private void OnSkillUnlocked(int attackIndex)
    {
        if (Application.isMobilePlatform) return;

        // W 스킬(index 2) 해금 시 W 슬롯 활성화
        if (attackIndex == 2 && wSlot != null)
            wSlot.SetActive(true);
    }

    private void Update()
    {
        if (swordAttackHandler != null)
        {
            // Q 입력 피드백
            float qRemaining = swordAttackHandler.Attack1ActiveRemaining;
            float qTotal = swordAttackHandler.AttackDuration1;
            SetOverlay(qAttackOverlay, qAttackText, qRemaining, qTotal);

            // W 쿨타임
            float wRemaining = swordAttackHandler.Attack2CooldownRemaining;
            float wTotal = swordAttackHandler.CooldownTime2;
            SetOverlay(wAttackOverlay, wAttackText, wRemaining, wTotal);
        }

        if (shieldHandler != null)
        {
            float sRemaining = shieldHandler.CooldownRemaining;
            float sTotal = shieldHandler.CooldownTime;
            SetOverlay(shieldOverlay, shieldText, sRemaining, sTotal);
        }
    }

    private void SetOverlay(Image overlay, TextMeshProUGUI text, float remaining, float total)
    {
        if (overlay == null) return;

        if (remaining > 0f && total > 0f)
        {
            overlay.fillAmount = remaining / total;
            if (text != null)
            {
                text.gameObject.SetActive(true);
                text.text = remaining.ToString("F1");
            }
        }
        else
        {
            overlay.fillAmount = 0f;
            if (text != null)
                text.gameObject.SetActive(false);
        }
    }
}
