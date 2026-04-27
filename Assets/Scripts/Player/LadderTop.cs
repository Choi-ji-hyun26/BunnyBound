using UnityEngine;

/// <summary>
/// 사다리 꼭대기 감지
/// - 사다리 상단에 얇은 Trigger
/// - OnTriggerStay2D로 매 프레임 체크
/// - 위로 이동 중일 때 플레이어를 플랫폼 위로 이동 후 Idle
/// - Layer: Ladder / IsTrigger: ON
/// </summary>
public class LadderTop : MonoBehaviour
{
    [SerializeField] private float exitOffsetY = 1f;   // 플레이어 절반 높이 + 여유값
    [SerializeField] private float exitVelocityY = 5f;  // 위로 밀어올리는 속도 (자연스러운 착지)

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
        if (sm.Input.ClimbInput.y <= 0.1f) return;

        Rigidbody2D rigid = sm.Coordinator.Rigid;

        // 강제 이동 대신 velocity로 위로 밀어올리기
        // 중력이 복원되면서 플랫폼 위에 자연스럽게 착지
        rigid.position = new Vector2(rigid.position.x, transform.position.y + exitOffsetY);
        rigid.velocity = new Vector2(0f, exitVelocityY);

        sm.SetOnLadder(false);
        sm.ChangeState(sm.IdleState);
    }

    // ───────────────────────────────────────────
    // Gizmo — 초록색 (꼭대기 영역)
    // ───────────────────────────────────────────
    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.35f);
        Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);

        Gizmos.color = new Color(0.2f, 1f, 0.3f, 1f);
        Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);

        #if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(0.1f, 0.8f, 0.2f, 1f);
        UnityEditor.Handles.Label(
            transform.position + (Vector3)col.offset,
            "  Top"
        );
        #endif
    }
}
