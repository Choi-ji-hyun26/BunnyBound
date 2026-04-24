using UnityEngine;

/// <summary>
/// 검사 공격 해금 시스템
/// - 기본 공격(Q)은 항상 해금
/// - W/E/R은 던전 보물상자 스킬북으로 해금
/// - Inspector에서 디버그 강제 해금 가능
/// </summary>
public class SkillUnlockManager : MonoBehaviour
{
    public static SkillUnlockManager Instance { get; private set; }

    [Header("해금 상태 (Inspector에서 확인/디버그 가능)")]
    [SerializeField] private bool attack1Unlocked = true;  // Q: 항상 해금
    [SerializeField] private bool attack2Unlocked = false; // W: 던전1 스킬북
    [SerializeField] private bool attack3Unlocked = false; // E: 던전2 스킬북
    [SerializeField] private bool attack4Unlocked = false; // R: 던전3 스킬북

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // ───────────────────────────────────────────
    // 해금 체크
    // ───────────────────────────────────────────

    public bool IsUnlocked(int attackIndex)
    {
        return attackIndex switch
        {
            1 => attack1Unlocked,
            2 => attack2Unlocked,
            3 => attack3Unlocked,
            4 => attack4Unlocked,
            _ => false
        };
    }

    // ───────────────────────────────────────────
    // 해금 처리 (보물상자 스킬북에서 호출)
    // ───────────────────────────────────────────

    public void UnlockAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 2: attack2Unlocked = true; break;
            case 3: attack3Unlocked = true; break;
            case 4: attack4Unlocked = true; break;
            default:
                Debug.LogWarning($"[SkillUnlock] 잘못된 공격 인덱스: {attackIndex}");
                break;
        }
        Debug.Log($"[SkillUnlock] Attack{attackIndex} 해금 완료!");
    }
}
