using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 레버 상호작용
/// - 검사 상태에서만 G키로 작동
/// - 한 번 작동 후 재조작 불가
/// - 애니메이션 종료 후 UnityEvent 발동 (타이밍 자연스럽게)
/// - LeverDetector 비활성화로 재진입 차단
/// </summary>
public class Lever : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent onActivated; // 레버 작동 완료 시

    [Header("Settings")]
    [SerializeField] private float animationDuration = 0.5f; // 애니메이션 길이에 맞게 조정
    [SerializeField] private float interactRange = 1.5f;

    private LeverDetector leverDetector;
    private Animator animator;
    private bool isActivated = false;
    private bool isAnimating = false;

    private void Awake()
    {
        leverDetector = GetComponentInChildren<LeverDetector>();
        animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("[Lever] Animator가 없습니다. 레버 애니메이션이 재생되지 않습니다.");
    }

    /// <summary>
    /// PlayerInteractionHandler에서 호출
    /// </summary>
    public void Activate()
    {
        if (isActivated || isAnimating) return;

        SoundManager.Instance.PlaySound("INTERACT");

        StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        isAnimating = true;

        // 레버 자체 애니메이션 재생
        if (animator != null)
            animator.SetTrigger("doActivate");

        // 애니메이션 종료 대기
        yield return new WaitForSeconds(animationDuration);

        isActivated = true;
        isAnimating = false;

        // Animator 비활성화 → 마지막 프레임 고정
        if (animator != null)
            animator.enabled = false;

        // LeverDetector 비활성화 (재조작 차단)
        if (leverDetector != null)
            leverDetector.gameObject.SetActive(false);

        // 애니메이션 끝난 후 효과 발동
        onActivated?.Invoke();

        Debug.Log("[Lever] 레버 작동 완료!");
    }

    public bool IsInRange(Vector2 playerPos)
    {
        return Vector2.Distance(transform.position, playerPos) <= interactRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
