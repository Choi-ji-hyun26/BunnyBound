using System.Collections;
using UnityEngine;

/// <summary>
/// 검사 전용 공격 시스템
/// - Q/W/E/R 4종 공격
/// - 1개 예약 버퍼링 (공격 중 다음 공격 1개 예약)
/// - 공격마다 별도 HitBox 활성화
/// - 해금된 공격만 사용 가능
/// - 공격 시작 시점에 플레이어 방향(flipX) 반영 → HitBox localPosition.x 부호 전환
/// </summary>
public class PlayerSwordAttackHandler : MonoBehaviour
{
    [Header("HitBox (공격별 별도 콜라이더)")]
    [SerializeField] private Collider2D hitBox1; // Q: 위→아래 slash
    [SerializeField] private Collider2D hitBox2; // W: 아래→위 slash
    [SerializeField] private Collider2D hitBox3; // E: 찌르기
    [SerializeField] private Collider2D hitBox4; // R: 회전 공격

    [Header("데미지")]
    [SerializeField] private int damage1 = 10; // Q
    [SerializeField] private int damage2 = 15; // W
    [SerializeField] private int damage3 = 20; // E
    [SerializeField] private int damage4 = 8;  // R

    [Header("공격 지속 시간 (애니메이션과 맞춰서 조정)")]
    [SerializeField] private float attackDuration1 = 0.4f;
    [SerializeField] private float attackDuration2 = 0.4f;
    [SerializeField] private float attackDuration3 = 0.35f;
    [SerializeField] private float attackDuration4 = 0.5f;

    [Header("HitBox 활성화 타이밍 (0~1, 애니메이션 진행률 기준)")]
    [SerializeField] private float hitBoxStartRatio = 0.2f;
    [SerializeField] private float hitBoxEndRatio   = 0.7f;

    // 상태
    public bool IsAttacking { get; private set; } = false;
    private int bufferedAttack = -1; // -1 = 없음

    // HitBox 기본 localPosition.x 절댓값 저장 (Awake 시점 기준)
    // flipX에 따라 부호를 반전해서 방향 적용
    private float[] hitBoxDefaultOffsetX = new float[4];

    private PlayerCoordinator coordinator;
    private PlayerInputHandler input;
    private PlayerTransformHandler transformHandler;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        coordinator      = GetComponent<PlayerCoordinator>();
        input            = GetComponent<PlayerInputHandler>();
        transformHandler = GetComponent<PlayerTransformHandler>();
        animator         = coordinator.Animator;
        spriteRenderer   = coordinator.SpriteRenderer;

        // 각 HitBox의 기본 localPosition.x 절댓값 저장
        // Inspector에서 오른쪽 기준으로 설정해두면 flipX 시 자동으로 반전
        Collider2D[] boxes = { hitBox1, hitBox2, hitBox3, hitBox4 };
        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i] != null)
                hitBoxDefaultOffsetX[i] = Mathf.Abs(boxes[i].transform.localPosition.x);
        }

        DisableAllHitBoxes();
    }

    private void Update()
    {
        if (transformHandler.currentType != CharacterType.Knight) return;
        HandleAttackInput();
    }

    // ───────────────────────────────────────────
    // 입력 처리
    // ───────────────────────────────────────────
    private void HandleAttackInput()
    {
        int pressedAttack = GetPressedAttack();
        if (pressedAttack == -1) return;

        if (!SkillUnlockManager.Instance.IsUnlocked(pressedAttack))
        {
            Debug.Log($"[SwordAttack] Attack{pressedAttack}는 아직 해금되지 않았습니다.");
            return;
        }

        if (!IsAttacking)
            StartCoroutine(ExecuteAttack(pressedAttack));
        else
            bufferedAttack = pressedAttack;
    }

    private int GetPressedAttack()
    {
        if (input.Attack1Pressed) return 1;
        if (input.Attack2Pressed) return 2;
        if (input.Attack3Pressed) return 3;
        if (input.Attack4Pressed) return 4;
        return -1;
    }

    // ───────────────────────────────────────────
    // 공격 실행
    // ───────────────────────────────────────────
    private IEnumerator ExecuteAttack(int attackIndex)
    {
        IsAttacking = true;

        // 공격 시작 시점에 방향 반영
        // 공격 중 방향 전환은 허용하지 않으므로 시작 시점 한 번만 처리
        ApplyHitBoxDirection();

        float duration = GetDuration(attackIndex);
        Collider2D hitBox = GetHitBox(attackIndex);

        animator.SetInteger("attackIndex", attackIndex);
        animator.SetTrigger("doAttack");

        float hitStart = duration * hitBoxStartRatio;
        float hitEnd   = duration * hitBoxEndRatio;

        yield return new WaitForSeconds(hitStart);
        ActivateHitBox(hitBox, attackIndex);

        yield return new WaitForSeconds(hitEnd - hitStart);
        DeactivateHitBox(hitBox);

        yield return new WaitForSeconds(duration - hitEnd);

        IsAttacking = false;

        if (bufferedAttack != -1)
        {
            int next = bufferedAttack;
            bufferedAttack = -1;

            if (SkillUnlockManager.Instance.IsUnlocked(next))
                StartCoroutine(ExecuteAttack(next));
        }
    }

    // ───────────────────────────────────────────
    // HitBox 방향 적용
    // flipX == true  → 왼쪽: localPosition.x = -절댓값
    // flipX == false → 오른쪽: localPosition.x = +절댓값
    // ───────────────────────────────────────────
    private void ApplyHitBoxDirection()
    {
        bool facingLeft = spriteRenderer.flipX;
        Collider2D[] boxes = { hitBox1, hitBox2, hitBox3, hitBox4 };

        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i] == null) continue;

            Vector3 pos = boxes[i].transform.localPosition;
            pos.x = facingLeft ? -hitBoxDefaultOffsetX[i] : hitBoxDefaultOffsetX[i];
            boxes[i].transform.localPosition = pos;
        }
    }

    // ───────────────────────────────────────────
    // HitBox 제어
    // ───────────────────────────────────────────
    private void ActivateHitBox(Collider2D hitBox, int attackIndex)
    {
        if (hitBox == null) return;
        hitBox.enabled = true;

        SwordHitBox swordHitBox = hitBox.GetComponent<SwordHitBox>();
        if (swordHitBox != null)
            swordHitBox.SetDamage(GetDamage(attackIndex));

        CheckBreakables(hitBox);
    }

    // ───────────────────────────────────────────
    // Breakable 오브젝트 탐색 및 파괴
    // ───────────────────────────────────────────
    private void CheckBreakables(Collider2D hitBox)
    {
        BoxCollider2D box = hitBox as BoxCollider2D;
        if (box == null) return;

        Vector2 center = (Vector2)hitBox.transform.position + box.offset;
        Vector2 size = box.size;
        float angle = hitBox.transform.eulerAngles.z;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Breakable")) continue;
            IBreakable breakable = hit.GetComponent<IBreakable>();
            breakable?.OnBreak();
        }
    }

    private void DeactivateHitBox(Collider2D hitBox)
    {
        if (hitBox == null) return;
        hitBox.enabled = false;
    }

    private void DisableAllHitBoxes()
    {
        if (hitBox1 != null) hitBox1.enabled = false;
        if (hitBox2 != null) hitBox2.enabled = false;
        if (hitBox3 != null) hitBox3.enabled = false;
        if (hitBox4 != null) hitBox4.enabled = false;
    }

    // ───────────────────────────────────────────
    // 유틸리티
    // ───────────────────────────────────────────
    private float GetDuration(int index) => index switch
    {
        1 => attackDuration1,
        2 => attackDuration2,
        3 => attackDuration3,
        4 => attackDuration4,
        _ => 0.4f
    };

    private int GetDamage(int index) => index switch
    {
        1 => damage1,
        2 => damage2,
        3 => damage3,
        4 => damage4,
        _ => 10
    };

    private Collider2D GetHitBox(int index) => index switch
    {
        1 => hitBox1,
        2 => hitBox2,
        3 => hitBox3,
        4 => hitBox4,
        _ => null
    };
}
