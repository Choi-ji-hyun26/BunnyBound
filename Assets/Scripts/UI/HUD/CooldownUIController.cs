using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// W 공격 / 쉴드 쿨타임 UI
///
/// [구조]
/// Canvas → SkillSlotContainer
///   → SlotW
///     → IconImage      (Image — 스킬 아이콘)
///     → OverlayImage   (Image — fillAmount, 검은 반투명, ImageType: Filled, FillMethod: Radial360)
///     → CooldownText   (TextMeshPro — 남은 시간 표시)
///   → SlotShield
///     → (동일 구조)
///
/// [OverlayImage 설정]
/// Image Type: Filled / Fill Method: Radial360 / Fill Origin: Top / Clockwise: true
/// Color: (0, 0, 0, 0.55)
///
/// [동작]
/// 쿨타임 중: OverlayImage.fillAmount = 잔여/전체, CooldownText 표시
/// 쿨타임 종료: OverlayImage.fillAmount = 0, CooldownText 비활성화
/// </summary>
public class CooldownUIController : MonoBehaviour
{
    [Header("W 공격 슬롯")]
    [SerializeField] private Image wAttackOverlay;
    [SerializeField] private TextMeshProUGUI wAttackText;

    [Header("쉴드 슬롯")]
    [SerializeField] private Image shieldOverlay;
    [SerializeField] private TextMeshProUGUI shieldText;

    [Header("참조")]
    [SerializeField] private PlayerSwordAttackHandler swordAttackHandler;
    [SerializeField] private PlayerShieldHandler shieldHandler;

    private void Awake()
    {
        if (swordAttackHandler == null)
            Debug.LogError("[CooldownUIController] swordAttackHandler가 연결되지 않았습니다.");
        if (shieldHandler == null)
            Debug.LogError("[CooldownUIController] shieldHandler가 연결되지 않았습니다.");

        SetOverlay(wAttackOverlay, wAttackText, 0f, 0f);
        SetOverlay(shieldOverlay, shieldText, 0f, 0f);
    }

    private void Update()
    {
        if (swordAttackHandler != null)
        {
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
