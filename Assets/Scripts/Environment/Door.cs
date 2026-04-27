using UnityEngine;

/// <summary>
/// 레버에 의해 열리는 문
/// - Lever의 UnityEvent에 Open()/Close() 연결
/// - 열림/닫힘 시 스프라이트 교체
/// - 콜라이더 없음 (포탈 비활성화로 진행 차단)
/// - 영구적으로 열림 or 토글 방식 (Lever 설정에 따라)
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite closedSprite; // 닫힌 문
    [SerializeField] private Sprite openSprite;   // 열린 문

    [Header("Components")]
    [SerializeField] private SpriteRenderer doorSprite;

    private bool isOpen = false;

    private void Awake()
    {
        if (doorSprite == null)
            doorSprite = GetComponent<SpriteRenderer>();

        // 시작 시 닫힌 상태
        Close();
    }

    /// <summary>
    /// Lever.onActivated에 연결
    /// </summary>
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (doorSprite != null && openSprite != null)
            doorSprite.sprite = openSprite;

        Debug.Log("[Door] 문이 열렸습니다!");
    }

    /// <summary>
    /// Lever.onDeactivated에 연결 (선택)
    /// </summary>
    public void Close()
    {
        isOpen = false;

        if (doorSprite != null && closedSprite != null)
            doorSprite.sprite = closedSprite;

        Debug.Log("[Door] 문이 닫혔습니다!");
    }
}
