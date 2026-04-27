using UnityEngine;

/// <summary>
/// 사다리 바닥 감지
/// - 사다리 하단에 얇은 Trigger
/// - OnTriggerStay2D로 매 프레임 체크
/// - 아래로 이동 중일 때 Idle 전환
/// - Layer: Ladder / IsTrigger: ON
/// </summary>
public class LadderBottom : MonoBehaviour
{
    private int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;

        PlayerStateMachine sm = other.GetComponentInParent<PlayerStateMachine>();
        if (sm == null) return;

        if (!(sm.CurrentState is ClimbState)) return;
        if (sm.Input.ClimbInput.y >= -0.1f) return;

        Rigidbody2D rigid = sm.Coordinator.Rigid;
        rigid.velocity = Vector2.zero;

        sm.SetOnLadder(false);
        sm.ChangeState(sm.IdleState);
    }

    // ───────────────────────────────────────────
    // Gizmo — 빨간색 (바닥 영역)
    // ───────────────────────────────────────────
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
