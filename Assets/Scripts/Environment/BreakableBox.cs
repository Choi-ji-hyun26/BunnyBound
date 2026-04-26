using System.Collections;
using UnityEngine;

/// <summary>
/// 파괴 가능한 박스
/// - 검사 공격(SwordHitBox)에만 반응 (Tag: Breakable)
/// - HP 2: 1회 피격 시 Damaged 스프라이트, 2회 피격 시 Break 스프라이트 후 제거
/// - 스케일 조정으로 세로/가로 다양한 배치 가능
/// </summary>
public class BreakableBox : MonoBehaviour, IBreakable
{
    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;   // Box1: 정상
    [SerializeField] private Sprite damagedSprite;  // Box2: 금 간 상태
    [SerializeField] private Sprite breakSprite;    // Box3: 파괴 이펙트

    [Header("Settings")]
    [SerializeField] private int maxHp = 2;
    [SerializeField] private float breakDelay = 0.3f; // 파괴 스프라이트 표시 시간

    private int currentHp;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool isBreaking = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        currentHp = maxHp;

        if (spriteRenderer != null && normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    /// <summary>
    /// SwordHitBox에서 Tag: Breakable 감지 시 호출
    /// </summary>
    public void OnBreak()
    {
        if (isBreaking) return;

        currentHp--;

        if (currentHp <= 0)
            StartCoroutine(BreakRoutine());
        else
            OnDamaged();
    }

    // ───────────────────────────────────────────
    // 피격 (HP 1 남음)
    // ───────────────────────────────────────────
    private void OnDamaged()
    {
        if (damagedSprite != null)
            spriteRenderer.sprite = damagedSprite;
    }

    // ───────────────────────────────────────────
    // 파괴 (HP 0)
    // ───────────────────────────────────────────
    private IEnumerator BreakRoutine()
    {
        isBreaking = true;

        // 파괴 스프라이트로 교체
        if (breakSprite != null)
            spriteRenderer.sprite = breakSprite;

        // 콜라이더 즉시 비활성화 (플레이어가 바로 통과 가능)
        if (boxCollider != null)
            boxCollider.enabled = false;

        // 잠시 파괴 이펙트 보여주고 제거
        yield return new WaitForSeconds(breakDelay);

        Destroy(gameObject);
    }
}
