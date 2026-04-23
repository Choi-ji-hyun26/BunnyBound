using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LadderDetector : MonoBehaviour
{
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.35f);

    private BoxCollider2D boxCol;

    private void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();

        if (stateMachine == null)
            stateMachine = GetComponentInParent<PlayerStateMachine>();

        if (stateMachine == null)
            Debug.LogError("LadderDetector: PlayerStateMachine 참조를 찾지 못했습니다.", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ladder ladder = other.GetComponentInParent<Ladder>();
        if (ladder != null)
            stateMachine.SetCurrentLadder(ladder);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Ladder ladder = other.GetComponentInParent<Ladder>();
        if (ladder != null)
            stateMachine.SetCurrentLadder(ladder);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Ladder ladder = other.GetComponentInParent<Ladder>();
        if (ladder != null)
            stateMachine.ClearCurrentLadder(ladder);
    }

    private void OnDrawGizmos()
    {
        DrawBoxGizmo();
    }

    private void OnDrawGizmosSelected()
    {
        DrawBoxGizmo();
    }

    private void DrawBoxGizmo()
    {
        BoxCollider2D col = boxCol != null ? boxCol : GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.color = gizmoColor;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.offset, col.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(col.offset, col.size);
        Gizmos.matrix = oldMatrix;
    }
}