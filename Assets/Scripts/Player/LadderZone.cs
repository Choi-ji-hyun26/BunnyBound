using UnityEngine;

/// <summary>
/// 사다리 메인 Zone — IsOnLadder 관리 및 ClimbState 진입 전담
/// - 진입: IsOnLadder = true
/// - 이탈: IsOnLadder = false → ClimbState면 자동 전환
/// - 위 입력: ClimbState 진입
/// - 아래 입력: 공중일 때만 ClimbState 진입
///   지상 아래 입력은 LadderTop 담당 — 실행 순서 보장을 위해 지상 체크 유지
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

        // 위 입력: 항상 진입
        // SetOnLadder(true) 명시 호출 — 드롭 텔레포트 후 IsOnLadder가 False로 남는 경우 보장
        if (climbY > 0.1f)
        {
            sm.SetOnLadder(true);
            sm.ChangeState(sm.ClimbState);
            return;
        }

        // 아래 입력: 공중일 때만 진입
        // 지상 아래 입력은 LadderTop이 텔레포트 후 처리
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
