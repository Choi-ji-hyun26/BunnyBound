using UnityEngine;

/// <summary>
/// 이동하는 발판
/// - 시작 위치에서 moveOffset만큼 왕복 이동
/// - startPos/endPos 빈 오브젝트 불필요
/// - Inspector에서 moveOffset과 speed만 설정
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector2 moveOffset = new Vector2(5f, 0f);
    [SerializeField] private float speed = 3f;
    [SerializeField] private float colliderHeight = 1f;

    private Vector2 startPos;
    private Vector2 endPos;
    private Vector2 targetPos;

    private Rigidbody2D rigid;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    private bool isInitialized = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        startPos = transform.position;
        endPos = startPos + moveOffset;
        targetPos = endPos;
        isInitialized = true;

        // SpriteRenderer Width에 맞게 콜라이더 크기 자동 조정
        if (boxCollider != null && spriteRenderer != null)
        {
            float width = spriteRenderer.size.x;
            boxCollider.size = new Vector2(width, colliderHeight);
            boxCollider.offset = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        Vector2 newPos = Vector2.MoveTowards(rigid.position, targetPos, speed * Time.fixedDeltaTime);
        rigid.MovePosition(newPos);

        // 목적지 도착 시 반전
        if (Vector2.Distance(rigid.position, targetPos) <= 0.05f)
            targetPos = (targetPos == endPos) ? startPos : endPos;
    }

    private void OnEnable()
    {
        // Start() 이전에 호출되면 무시 (startPos 미초기화 방지)
        if (!isInitialized) return;

        if (rigid != null)
        {
            rigid.position = startPos;
            rigid.velocity = Vector2.zero;
        }

        targetPos = endPos;
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터에서 이동 경로 시각화
        Vector2 start = transform.position;
        Vector2 end = start + moveOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(start, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(end, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, end);
    }
}
