using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 화면 상단 중앙 알림 메시지 UI
/// - 스킬북 획득 등 즉시 알림이 필요한 상황에서 호출
/// - 5초 후 자동 페이드아웃 또는 아무 키 입력 시 즉시 닫힘
/// </summary>
public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    [Header("Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 5f;  // 자동 닫힘 시간
    [SerializeField] private float fadeDuration = 0.3f;   // 페이드 속도

    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        SetAlpha(0f);
    }

    private void Update()
    {
        // 메시지 표시 중 아무 키 입력 시 즉시 닫힘
        if (canvasGroup.alpha > 0f && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            ForceClose();
    }

    // ───────────────────────────────────────────
    // 외부 호출 — 메시지 표시
    // ───────────────────────────────────────────
    public void Show(string message)
    {
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        messageText.text = message;
        notificationCoroutine = StartCoroutine(NotificationRoutine());
    }

    // ───────────────────────────────────────────
    // 즉시 닫기
    // ───────────────────────────────────────────
    private void ForceClose()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }

        notificationCoroutine = StartCoroutine(FadeOut());
    }

    // ───────────────────────────────────────────
    // 알림 루틴: 페이드인 → 대기 → 페이드아웃
    // ───────────────────────────────────────────
    private IEnumerator NotificationRoutine()
    {
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSecondsRealtime(displayDuration);
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, timer / fadeDuration));
            yield return null;
        }
        SetAlpha(1f);
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        float startAlpha = canvasGroup.alpha;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, 0f, timer / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.blocksRaycasts = alpha > 0.1f;
    }
}
