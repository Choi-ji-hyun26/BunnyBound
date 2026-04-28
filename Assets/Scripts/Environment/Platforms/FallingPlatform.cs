using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 떨어지는 발판
/// - 플레이어가 위에서 밟으면 fallDelay 후 낙하
/// - respawnDelay 후 원래 위치로 복귀
/// 
/// [Prefab 구성]
/// FallingPlatform (Rigidbody2D Static, BoxCollider2D)
/// ├── SpriteRenderer
/// └── FallingPlatform.cs
/// 
/// [씬 배치]
/// FallingPlatforms (빈 부모 오브젝트)
/// ├── FallingPlatform (Prefab 인스턴스)
/// ├── FallingPlatform (Prefab 인스턴스)
/// └── ...
/// </summary>
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay = 0.5f;   // 밟고 나서 떨어지는 시간
    [SerializeField] private float respawnDelay = 3f;  // 다시 생성되는 시간

    private Rigidbody2D rigid;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isFalling = false;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
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
