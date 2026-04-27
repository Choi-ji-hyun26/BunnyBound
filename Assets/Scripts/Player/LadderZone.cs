using UnityEngine;

/// <summary>
/// 사다리 메인 Zone
/// - 사다리 전체 높이의 Trigger
/// - OnTriggerStay2D로 매 프레임 플레이어 감지
/// - 위/아래 입력 있으면 ClimbState 진입
/// - Layer: Ladder / IsTrigger: ON
/// </summary>
public class LadderZone : MonoBehaviour
{
    private int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;

        PlayerStateMachine sm = other.GetComponentInParent<PlayerStateMachine>();
        if (sm != null) sm.SetOnLadder(true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;

        PlayerStateMachine sm = other.GetComponentInParent<PlayerStateMachine>();
        if (sm == null) return;

        if (sm.CurrentState is ClimbState) return;

        float climbY = sm.Input.ClimbInput.y;

        if (climbY > 0.1f)
        {
            sm.SetOnLadder(true);
            sm.ChangeState(sm.ClimbState);
            return;
        }

        if (climbY < -0.1f && !sm.IsGroundedCached)
        {
            sm.SetOnLadder(true);
            sm.ChangeState(sm.ClimbState);
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;

        PlayerStateMachine sm = other.GetComponentInParent<PlayerStateMachine>();
        if (sm != null) sm.SetOnLadder(false);
    }

    // ───────────────────────────────────────────
    // Gizmo — 파란색 (Zone 전체 영역)
    // ───────────────────────────────────────────
    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.25f);
        Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);

        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.9f);
        Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);

        #if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(0.2f, 0.5f, 1f, 1f);
        UnityEditor.Handles.Label(
            transform.position + (Vector3)col.offset,
            "  Zone"
        );
        #endif
    }
}
