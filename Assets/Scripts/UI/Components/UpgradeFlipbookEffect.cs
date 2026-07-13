using UnityEngine;

/// <summary>
/// 강화 성공 시 재생되는 플립북(프레임 순차 재생) 이펙트 — Animator 기반
/// - Animation 창에서 Image.sprite 프로퍼티에 프레임별 키프레임을 찍어 재생
/// - Animator.updateMode를 UnscaledTime으로 강제 설정해 Time.timeScale = 0
///   (강화 창 일시정지) 상태에서도 재생되도록 보장 — Inspector 설정을 놓쳐도
///   Awake에서 재설정하므로 안전
/// - 클립 마지막 프레임에 Animation Event로 OnAnimationFinished()를 연결해
///   재생 종료 시 자동으로 비활성화
/// </summary>
public class UpgradeFlipbookEffect : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string stateName = "UpgradeBurst"; // Animator Controller의 실제 상태(State) 이름과 일치해야 함

    private int stateHash;
    private bool stateValid;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("[UpgradeFlipbookEffect] Animator 컴포넌트가 없습니다.");
        }
        else
        {
            // Time.timeScale = 0 상태에서도 재생되도록 보장
            // (Inspector에서 Update Mode를 Unscaled Time으로 설정해도 되지만
            // 놓쳤을 때를 대비한 방어적 재설정)
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            stateHash = Animator.StringToHash(stateName);

            stateValid = animator.HasState(0, stateHash);
            if (!stateValid)
                Debug.LogError($"[UpgradeFlipbookEffect] Animator에 '{stateName}' 상태가 없습니다. stateName을 확인하세요.");
        }

        gameObject.SetActive(false); // 평소엔 숨김
    }

    public void Play()
    {
        if (!stateValid)
        {
            Debug.LogError($"[UpgradeFlipbookEffect] '{stateName}' 상태가 유효하지 않아 재생을 건너뜁니다.");
            return;
        }

        gameObject.SetActive(true);
        // normalizedTime 0f로 매번 처음부터 강제 재시작
        // (GameObject를 비활성화/활성화할 때 Animator가 진행률을 이어가는 케이스를 방지)
        animator.Play(stateHash, 0, 0f);
    }

    /// <summary>
    /// 플레이 모드에서 Inspector 컴포넌트명 우클릭 -> 이 메뉴로 강화 경제(별 소비/tier 증가)
    /// 없이 이펙트만 단독으로 검증할 때 사용 -- 세이브 데이터를 전혀 건드리지 않음
    /// </summary>
    [ContextMenu("Test Play")]
    private void TestPlay() => Play();

    /// <summary>
    /// Animation Event 전용 — 클립 마지막 프레임에 연결해 재생 종료 시 자동 호출
    /// </summary>
    public void OnAnimationFinished()
    {
        gameObject.SetActive(false);
    }
}
