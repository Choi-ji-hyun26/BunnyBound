using System.Collections;
using UnityEngine;

/// <summary>
/// 검사 전용 공격 시스템
/// - Q/W/E/R 4종 공격
/// - 1개 예약 버퍼링 (공격 중 다음 공격 1개 예약)
/// - 공격마다 별도 HitBox 활성화
/// - 해금된 공격만 사용 가능
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

    private PlayerCoordinator coordinator;
    private PlayerInputHandler input;
    private PlayerTransformHandler transformHandler;
    private Animator animator;

    private void Awake()
    {
        coordinator      = GetComponent<PlayerCoordinator>();
        input            = GetComponent<PlayerInputHandler>();
        transformHandler = GetComponent<PlayerTransformHandler>();
        animator         = coordinator.Animator;

        // 시작 시 모든 HitBox 비활성화
        DisableAllHitBoxes();
    }

    private void Update()
    {
        // 검사 상태일 때만 공격 처리
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

        // 해금 체크
        if (!SkillUnlockManager.Instance.IsUnlocked(pressedAttack))
        {
            Debug.Log($"[SwordAttack] Attack{pressedAttack}는 아직 해금되지 않았습니다.");
            return;
        }

        if (!IsAttacking)
        {
            // 공격 중이 아니면 즉시 실행
            StartCoroutine(ExecuteAttack(pressedAttack));
        }
        else
        {
            // 공격 중이면 1개 예약 (덮어쓰기)
            bufferedAttack = pressedAttack;
        }
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

        float duration = GetDuration(attackIndex);
        Collider2D hitBox = GetHitBox(attackIndex);

        // 애니메이션 트리거 (attackIndex 세팅 후 doAttack Trigger)
        animator.SetInteger("attackIndex", attackIndex);
        animator.SetTrigger("doAttack");

        // HitBox 활성화 타이밍
        float hitStart = duration * hitBoxStartRatio;
        float hitEnd   = duration * hitBoxEndRatio;

        yield return new WaitForSeconds(hitStart);
        ActivateHitBox(hitBox, attackIndex);

        yield return new WaitForSeconds(hitEnd - hitStart);
        DeactivateHitBox(hitBox);

        yield return new WaitForSeconds(duration - hitEnd);

        IsAttacking = false;

        // 예약된 공격 실행
        if (bufferedAttack != -1)
        {
            int next = bufferedAttack;
            bufferedAttack = -1;

            if (SkillUnlockManager.Instance.IsUnlocked(next))
                StartCoroutine(ExecuteAttack(next));
        }
    }

    // ───────────────────────────────────────────
    // HitBox 제어
    // ───────────────────────────────────────────

    private void ActivateHitBox(Collider2D hitBox, int attackIndex)
    {
        if (hitBox == null) return;
        hitBox.enabled = true;

        // HitBox에 데미지 전달
        SwordHitBox swordHitBox = hitBox.GetComponent<SwordHitBox>();
        if (swordHitBox != null)
            swordHitBox.SetDamage(GetDamage(attackIndex));

        // OverlapBox로 Breakable 오브젝트 직접 탐색 및 파괴
        // Trigger 방식의 Enter/Stay 문제를 우회
        CheckBreakables(hitBox);
    }

    // ───────────────────────────────────────────
    // Breakable 오브젝트 탐색 및 파괴
    // Physics2D.OverlapBoxAll로 HitBox 범위 내 탐색
    // ───────────────────────────────────────────
    private void CheckBreakables(Collider2D hitBox)
    {
        // HitBox의 실제 월드 크기와 위치로 OverlapBox 탐색
        BoxCollider2D box = hitBox as BoxCollider2D;
        if (box == null) return;

        Vector2 center = (Vector2)hitBox.transform.position + box.offset;
        Vector2 size = box.size;
        float angle = hitBox.transform.eulerAngles.z;

        // Breakable 레이어 마스크 없이 태그로 필터링
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
