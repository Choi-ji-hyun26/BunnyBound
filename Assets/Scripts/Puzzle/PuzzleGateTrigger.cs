using UnityEngine;

/// <summary>
/// 퍼즐 문 앞 트리거
/// - 플레이어 진입 시 PlayerInteractionHandler에 등록
/// - G키 입력 시 PuzzleGateUI.Open(sequenceGate) 호출
/// - 토끼/검사 둘 다 상호작용 가능
///
/// [인스펙터 설정]
/// - sequenceGate: SequenceGate 컴포넌트 참조
/// - puzzleGateUI: PuzzleGateUI 컴포넌트 참조
/// </summary>
public class PuzzleGateTrigger : MonoBehaviour
{
    [SerializeField] private SequenceGate sequenceGate;
    [SerializeField] private PuzzleGateUI puzzleGateUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerInteractionHandler>()
            ?.SetNearbyPuzzleGate(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerInteractionHandler>()
            ?.ClearNearbyPuzzleGate();
    }

    public void OpenPuzzleUI()
    {
        puzzleGateUI.Open(sequenceGate);
    }
}
