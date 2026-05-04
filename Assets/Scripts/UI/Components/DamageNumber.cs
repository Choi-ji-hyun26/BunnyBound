using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 데미지 숫자 UI
/// - 피격 위치에서 스폰 → 위로 이동하며 페이드아웃 → 풀에 반환
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float moveSpeed  = 1.5f; // 위로 이동 속도
    [SerializeField] private float duration   = 0.7f; // 전체 표시 시간
    [SerializeField] private float fadeStart  = 0.4f; // 페이드 시작 시점 (duration 기준 비율)

    private Coroutine animCoroutine;

    public void Play(int damage, Vector3 worldPosition)
    {
        transform.position = worldPosition;
        text.text          = damage.ToString();
        text.color         = new Color(text.color.r, text.color.g, text.color.b, 1f);
        gameObject.SetActive(true);

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        float elapsed  = 0f;
        float fadeTime = duration * (1f - fadeStart);
        Color color    = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 위로 이동
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // 페이드아웃 — fadeStart 비율 이후부터 알파 감소
            float fadeProgress = Mathf.InverseLerp(duration * fadeStart, duration, elapsed);
            color.a            = Mathf.Lerp(1f, 0f, fadeProgress);
            text.color         = color;

            yield return null;
        }

        // 풀에 반환
        DamageNumberPool.Instance.Return(this);
    }
}
