using UnityEngine;

/// <summary>
/// 사다리 바닥 감지
/// - ClimbState에서 아래 입력 시 FallState 전환
/// - 아래 지형이 OneWayPlatform인 경우 platformEffector 연결 (옵셔널)
/// - Layer: Ladder / IsTrigger: ON
/// </summary>
public class LadderBottom : MonoBehaviour
{
    [SerializeField] private PlatformEffector2D platformEffector;

    private int playerLayer;
    private int playerLayerBit;

    private void Awake()
    {
        playerLayer    = LayerMask.NameToLayer("Player");
        playerLayerBit = 1 << playerLayer;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;

        PlayerStateMachine sm = other.GetComponentInParent<PlayerStateMachine>();
        if (sm == null) return;
        if (!(sm.CurrentState is ClimbState)) return;
        if (sm.Input.ClimbInput.y >= -0.1f) return;

        if (platformEffector != null)
            platformEffector.colliderMask &= ~playerLayerBit;

        sm.ChangeState(sm.FallState);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;

        if (platformEffector != null)
            platformEffector.colliderMask |= playerLayerBit;
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);
        Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);

        #if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(1f, 0.1f, 0.1f, 1f);
        UnityEditor.Handles.Label(
            transform.position + (Vector3)col.offset,
            "  Bottom"
        );
        #endif
    }
}
