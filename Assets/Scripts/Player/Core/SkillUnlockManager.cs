using UnityEngine;

/// <summary>
/// 검사 공격 해금 시스템
/// - 해금 상태를 GameProgress에서 로드/저장
/// - 기본 공격(Q, index 1)은 항상 해금
/// - W(index 2)는 던전 보물상자 스킬북으로 해금
/// </summary>
public class SkillUnlockManager : MonoBehaviour
{
    public static SkillUnlockManager Instance { get; private set; }

    // 스킬 해금 이벤트 — CooldownUIController 등에서 구독
    public event System.Action<int> OnSkillUnlocked;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 저장 데이터 로드 보장
        GameProgress.Load();
    }

    // ───────────────────────────────────────────
    // 해금 체크 — GameProgress에서 직접 읽음
    // ───────────────────────────────────────────
    public bool IsUnlocked(int attackIndex)
    {
        return GameProgress.IsSkillUnlocked(attackIndex);
    }

    // ───────────────────────────────────────────
    // 해금 처리 — GameProgress에 저장
    // ───────────────────────────────────────────
    public void UnlockAttack(int attackIndex)
    {
        if (attackIndex != 2)
        {
            Debug.LogWarning($"[SkillUnlock] 잘못된 공격 인덱스: {attackIndex}");
            return;
        }

        GameProgress.UnlockSkill(attackIndex);
        OnSkillUnlocked?.Invoke(attackIndex);
        Debug.Log($"[SkillUnlock] Attack{attackIndex} 해금 완료!");
    }
}
