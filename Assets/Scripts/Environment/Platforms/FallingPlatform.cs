using UnityEngine;

/// <summary>
/// 떨어지는 발판
/// - 플레이어가 위에서 밟으면 fallDelay 후 낙하
/// - respawnDelay 후 원래 위치로 복귀
/// - SpriteRenderer Draw Mode: Tiled 사용 시 Width에 맞게 콜라이더 자동 조정
/// </summary>
public class FallingPlatform : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fallDelay = 0.2f;      // 밟고 나서 떨어지는 시간
    [SerializeField] private float respawnDelay = 3f;     // 복귀 시간
    [SerializeField] private float colliderHeight = 0.4f; // Wood: 0.4 / Stone: 1.0

    private Rigidbody2D rigid;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isFalling = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // 스테이지 전환 시 오브젝트가 다시 활성화될 때 완전히 리셋
        // Invoke가 대기 중이었다면 취소
        CancelInvoke();

        if (rigid != null)
        {
            rigid.bodyType = RigidbodyType2D.Static;
            rigid.velocity = Vector2.zero;
            rigid.angularVelocity = 0f;
        }

        if (originalPosition != Vector3.zero)
            transform.position = originalPosition;

        transform.rotation = originalRotation;
        isFalling = false;
    }

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // SpriteRenderer Width에 맞게 콜라이더 크기 자동 조정
        // Draw Mode: Tiled 사용 시 spriteRenderer.size.x가 실제 너비
        if (boxCollider != null && spriteRenderer != null)
        {
            float width = spriteRenderer.size.x;
            boxCollider.size = new Vector2(width, colliderHeight);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player가 아니면 무시
        if (!collision.gameObject.CompareTag("Player")) return;
        // 이미 떨어지는 중이면 무시
        if (isFalling) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 플레이어가 위에서 밟았을 때만 (접촉 법선이 위쪽)
            if (contact.normal.y < -0.5f)
            {
                isFalling = true;
                Invoke(nameof(Fall), fallDelay);
                break;
            }
        }
    }

    private void Fall()
    {
        rigid.bodyType = RigidbodyType2D.Dynamic;
        Invoke(nameof(ResetPlatform), respawnDelay);
    }

    private void ResetPlatform()
    {
        rigid.bodyType = RigidbodyType2D.Static;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        isFalling = false;
    }
}
