using UnityEngine;

/// <summary>
/// 사다리 꼭대기 감지
/// - 위로 이동 중(ClimbState): 플랫폼 표면 위로 스냅 후 Idle
/// - 아래 입력(Idle/Walk): 플레이어를 플랫폼 바로 아래로 텔레포트 후 ClimbState 직접 진입
/// - Layer: Ladder / IsTrigger: ON
///
/// [한계]
/// 드롭 후 즉시 위 입력 시 가끔 막히는 현상 미해결
/// 원인: PlatformEffector2D + Rigidbody2D Trigger 기반 구조의 물리 타이밍 경쟁
/// 근본 해결은 Raycast 기반 컨트롤러로의 전면 재설계 필요
/// </summary>
public class LadderTop : MonoBehaviour
{
    [SerializeField] private float exitOffsetPadding = 0.05f;
    [SerializeField] private GameObject oneWayPlatformObject;

    private int playerLayer;
    private BoxCollider2D ladderTopCol;
    private Collider2D platformCollider;
    private bool isProcessingDrop = false;
    private bool isDroppedFromTop = false;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        ladderTopCol = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        if (oneWayPlatformObject != null)
            platformCollider = oneWayPlatformObject.GetComponent<CompositeCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;

        // 드롭으로 인한 재진입이면 isProcessingDrop 유지 (같은 프레임 재처리 방지)
        if (isDroppedFromTop)
        {
            isDroppedFromTop = false;
            return;
        }

        isProcessingDrop = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;
        if (isProcessingDrop) return;

        PlayerStateMachine sm = other.GetComponentInParent<PlayerStateMachine>();
        if (sm == null) return;

        float climbY = sm.Input.ClimbInput.y;

        // ── 위로 올라오는 경우: 플랫폼 표면 위로 스냅 후 Idle ──
        if (sm.CurrentState is ClimbState && climbY > 0.1f)
        {
            SnapPlayerAbovePlatform(sm);
            return;
        }

        // ── 위에서 내려오는 경우: Idle/Walk 상태에서 아래 입력 ──
        bool isOnGround = sm.CurrentState is IdleState || sm.CurrentState is WalkState;
        if (isOnGround && climbY < -0.1f)
        {
            ExecuteDrop(sm);
        }
    }

    private void SnapPlayerAbovePlatform(PlayerStateMachine sm)
    {
        Rigidbody2D rigid = sm.Coordinator.Rigid;
        BoxCollider2D col = sm.Coordinator.BoxCollider;

        // 올라오기 스냅: platformCollider.bounds.max.y 기준
        // LadderTop Trigger 상단보다 실제 플랫폼 표면이 정확함
        float platformSurfaceY = platformCollider != null
            ? platformCollider.bounds.max.y
            : transform.position.y + ladderTopCol.offset.y + ladderTopCol.size.y * 0.5f;

        float colliderTop = col.size.y * 0.5f + col.offset.y;
        float snapY = platformSurfaceY + colliderTop + exitOffsetPadding;

        rigid.position = new Vector2(rigid.position.x, snapY);
        rigid.velocity = Vector2.zero;

        sm.IsOnLadder_ForceSet(false);
        sm.ChangeState(sm.IdleState);
    }

    private void ExecuteDrop(PlayerStateMachine sm)
    {
        isProcessingDrop = true;
        isDroppedFromTop = true; // OnTriggerEnter2D에서 isProcessingDrop 리셋 방지

        Rigidbody2D rigid = sm.Coordinator.Rigid;
        BoxCollider2D col = sm.Coordinator.BoxCollider;

        // 드롭: LadderTop Trigger 하단 기준
        // CompositeCollider2D bounds.max.y는 Tilemap 특성상 실제 표면과 오차 발생
        float platformSurfaceY = transform.position.y + ladderTopCol.offset.y - ladderTopCol.size.y * 0.5f;
        float colliderTop = col.size.y * 0.5f + col.offset.y;
        float targetY = platformSurfaceY - colliderTop - exitOffsetPadding;

        rigid.position = new Vector2(rigid.position.x, targetY);
        rigid.velocity = Vector2.zero;

        sm.IsOnLadder_ForceSet(true);
        sm.ChangeState(sm.ClimbState);
        // isProcessingDrop은 여기서 리셋하지 않음
        // → OnTriggerExit2D 또는 다음 비드롭 OnTriggerEnter2D에서 리셋
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;
        isProcessingDrop = false;
        isDroppedFromTop = false;
    }

    private void OnDrawGizmos()
    {
        if (ladderTopCol == null) return;

        Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.35f);
        Gizmos.DrawCube(transform.position + (Vector3)ladderTopCol.offset, ladderTopCol.size);

        Gizmos.color = new Color(0.2f, 1f, 0.3f, 1f);
        Gizmos.DrawWireCube(transform.position + (Vector3)ladderTopCol.offset, ladderTopCol.size);

        #if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(0.1f, 0.8f, 0.2f, 1f);
        UnityEditor.Handles.Label(
            transform.position + (Vector3)ladderTopCol.offset,
            "  Top"
        );
        #endif
    }
}
