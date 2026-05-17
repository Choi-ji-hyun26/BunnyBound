using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 모바일 스킬 버튼의 쿨다운 overlay를 제어합니다.
/// OnScreenButton과 함께 버튼 오브젝트에 붙입니다.
///
/// [사용법]
/// Inspector에서 SkillType을 설정하고
/// swordAttackHandler 또는 shieldHandler를 연결합니다.
/// </summary>
public class MobileSkillButton : MonoBehaviour
{
    public enum SkillType { Attack1, Attack2, Shield }

    [Header("스킬 타입")]
    [SerializeField] private SkillType skillType;

    [Header("Overlay")]
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("참조")]
    [SerializeField] private PlayerSwordAttackHandler swordAttackHandler;
    [SerializeField] private PlayerShieldHandler shieldHandler;

    private void Update()
    {
        switch (skillType)
        {
            case SkillType.Attack1:
                UpdateOverlay(
                    swordAttackHandler != null ? swordAttackHandler.Attack1ActiveRemaining : 0f,
                    swordAttackHandler != null ? swordAttackHandler.AttackDuration1 : 0f);
                break;

            case SkillType.Attack2:
                UpdateOverlay(
                    swordAttackHandler != null ? swordAttackHandler.Attack2CooldownRemaining : 0f,
                    swordAttackHandler != null ? swordAttackHandler.CooldownTime2 : 0f);
                break;

            case SkillType.Shield:
                UpdateOverlay(
                    shieldHandler != null ? shieldHandler.CooldownRemaining : 0f,
                    shieldHandler != null ? shieldHandler.CooldownTime : 0f);
                break;
        }
    }

    private void UpdateOverlay(float remaining, float total)
    {
        if (cooldownOverlay == null) return;

        if (remaining > 0f && total > 0f)
        {
            cooldownOverlay.fillAmount = remaining / total;
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = remaining.ToString("F1");
            }
        }
        else
        {
            cooldownOverlay.fillAmount = 0f;
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
        }
    }
}
