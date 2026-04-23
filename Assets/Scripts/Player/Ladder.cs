using UnityEngine;

public class Ladder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D triggerCollider;
    [SerializeField] private Transform topMountPoint;
    [SerializeField] private Transform bottomMountPoint;
    [SerializeField] private Collider2D topExitPlatform;

    [Header("Gizmos")]
    [SerializeField] private Color triggerFillColor = new Color(0f, 0.8f, 1f, 0.2f);
    [SerializeField] private Color triggerWireColor = Color.cyan;
    [SerializeField] private Color topPointColor = Color.yellow;
    [SerializeField] private Color bottomPointColor = Color.magenta;
    [SerializeField] private float pointRadius = 0.08f;

    public BoxCollider2D TriggerCollider => triggerCollider;
    public Transform TopMountPoint => topMountPoint;
    public Transform BottomMountPoint => bottomMountPoint;
    public Collider2D TopExitPlatform => topExitPlatform;

    public float CenterX => triggerCollider != null ? triggerCollider.bounds.center.x : transform.position.x;
    public float TopY => triggerCollider != null ? triggerCollider.bounds.max.y : transform.position.y;
    public float BottomY => triggerCollider != null ? triggerCollider.bounds.min.y : transform.position.y;

    private void Reset()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponentInChildren<BoxCollider2D>();
    }

    private void OnValidate()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponentInChildren<BoxCollider2D>();
    }

    private void OnDrawGizmos()
    {
        DrawLadderGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawLadderGizmos();
    }

    private void DrawLadderGizmos()
    {
        DrawTrigger();
        DrawMountPoint(topMountPoint, topPointColor, "Top");
        DrawMountPoint(bottomMountPoint, bottomPointColor, "Bottom");
        DrawCenterLine();
    }

    private void DrawTrigger()
    {
        if (triggerCollider == null) return;

        Gizmos.color = triggerFillColor;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = triggerCollider.transform.localToWorldMatrix;
        Gizmos.DrawCube(triggerCollider.offset, triggerCollider.size);

        Gizmos.color = triggerWireColor;
        Gizmos.DrawWireCube(triggerCollider.offset, triggerCollider.size);
        Gizmos.matrix = oldMatrix;
    }

    private void DrawMountPoint(Transform point, Color color, string label)
    {
        if (point == null) return;

        Gizmos.color = color;
        Gizmos.DrawSphere(point.position, pointRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.Label(point.position + Vector3.right * 0.08f, label);
#endif
    }

    private void DrawCenterLine()
    {
        if (triggerCollider == null) return;

        Bounds b = triggerCollider.bounds;
        Vector3 top = new Vector3(b.center.x, b.max.y, 0f);
        Vector3 bottom = new Vector3(b.center.x, b.min.y, 0f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(top, bottom);
    }
}