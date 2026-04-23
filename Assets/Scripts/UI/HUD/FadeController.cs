using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    public Image fadeImage;
    CanvasGroup fadeCanvasGroup;

    public float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        fadeCanvasGroup = fadeImage.GetComponent<CanvasGroup>();
        SetAlpha(0f); // 이미지 투명
        
    }

    // 알파값 직접 세팅 함수
    private void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;

        fadeCanvasGroup.blocksRaycasts = alpha > 0.1f; // 투명할 때는 클릭 통과
    }

    // 투명 -> 불투명
    public IEnumerator FadeOut() // public : GameManager.cs 에서 호출
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, timer / fadeDuration));
            yield return null;
        }
        SetAlpha(1f);
    }
    // 불투명 -> 투명
    public IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, timer / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
    }
    
}
