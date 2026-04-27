using UnityEngine;

/// <summary>
/// 레버 감지 Trigger
/// - 플레이어가 범위 안에 들어오면 PlayerInteractionHandler에 레버 등록
/// - 범위 밖으로 나가면 레버 해제
/// - Lever 오브젝트 하위에 배치 (CircleCollider2D, IsTrigger ON)
/// </summary>
public class LeverDetector : MonoBehaviour
{
    private Lever lever;

    private void Awake()
    {
        lever = GetComponentInParent<Lever>();
        if (lever == null)
            Debug.LogError("[LeverDetector] 부모에서 Lever를 찾을 수 없습니다.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInteractionHandler handler = other.GetComponentInParent<PlayerInteractionHandler>();
        if (handler != null)
            handler.SetNearbyLever(lever);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInteractionHandler handler = other.GetComponentInParent<PlayerInteractionHandler>();
        if (handler != null)
            handler.ClearNearbyLever();
    }
}
