using System.Collections;
using UnityEngine;

/// <summary>
/// 검사 전용 공격 시스템
/// - Q: 근접 slash (hitBox1)
/// - W: 원거리 slash 투사체 (SlashProjectile)
/// - E/R: 미사용 (추후 확장)
/// - 1개 예약 버퍼링
/// - 해금된 공격만 사용 가능
/// - 공격 시작 시점에 플레이어 방향(flipX) 반영
/// </summary>
public class PlayerSwordAttackHandler : MonoBehaviour
{
    [Header("HitBox")]
    [SerializeField] private Collider2D hitBox1; // Q: 근접 slash

    [Header("W: 원거리 Slash 투사체")]
    [SerializeField] private GameObject slashProjectilePrefab; // SlashProjectile Prefab
    [SerializeField] private Transform hitBox2Transform;       // 투사체 생성 위치 (기존 hitBox2 위치)

    [Header("데미지")]
    [SerializeField] private int damage1 = 10; // Q
    [SerializeField] private int damage2 = 15; // W

    [Header("W 쿨타임")]
    [SerializeField] private float cooldownTime2 = 2.5f;
    public bool IsAttack2OnCooldown { get; private set; } = false;
    public float Attack2CooldownRemaining { get; private set; } = 0f;

    [Header("공격 지속 시간")]
    [SerializeField] private float attackDuration1 = 0.4f;
    [SerializeField] private float attackDuration2 = 0.4f;

    [Header("HitBox 활성화 타이밍 (0~1, 애니메이션 진행률 기준)")]
    [SerializeField] private float hitBoxStartRatio = 0.2f;
    [SerializeField] private float hitBoxEndRatio   = 0.7f;

    public bool IsAttacking { get; private set; } = false;
    private int bufferedAttack = -1;

    private float[] hitBoxDefaultOffsetX = new float[1]; // hitBox1만 관리

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

        if (hitBox1 != null)
            hitBoxDefaultOffsetX[0] = Mathf.Abs(hitBox1.transform.localPosition.x);

        if (hitBox1 != null) hitBox1.enabled = false;
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

        // W 쿨타임 체크
        if (pressedAttack == 2 && IsAttack2OnCooldown) return;

        if (!IsAttacking)
            StartCoroutine(ExecuteAttack(pressedAttack));
        else
            bufferedAttack = pressedAttack;
    }

    private int GetPressedAttack()
    {
        if (input.Attack1Pressed) return 1;
        if (input.Attack2Pressed) return 2;
        return -1;
    }

    // ───────────────────────────────────────────
    // 공격 실행
    // ───────────────────────────────────────────
    private IEnumerator ExecuteAttack(int attackIndex)
    {
        IsAttacking = true;

        ApplyHitBoxDirection();

        float duration = GetDuration(attackIndex);

        animator.SetInteger("attackIndex", attackIndex);
        animator.SetTrigger("doAttack");

        if (attackIndex == 2)
        {
            // W: 원거리 투사체 — hitStart 타이밍에 생성
            float hitStart = duration * hitBoxStartRatio;
            yield return new WaitForSeconds(hitStart);
            SpawnSlashProjectile();
            yield return new WaitForSeconds(duration - hitStart);

            // W 쿨타임 시작
            StartCoroutine(Attack2CooldownRoutine());
        }
        else
        {
            // Q: 근접 HitBox 활성화
            Collider2D hitBox = GetHitBox(attackIndex);
            float hitStart = duration * hitBoxStartRatio;
            float hitEnd   = duration * hitBoxEndRatio;

            yield return new WaitForSeconds(hitStart);
            ActivateHitBox(hitBox, attackIndex);

            yield return new WaitForSeconds(hitEnd - hitStart);
            DeactivateHitBox(hitBox);

            yield return new WaitForSeconds(duration - hitEnd);
        }

        IsAttacking = false;

        if (bufferedAttack != -1)
        {
            int next = bufferedAttack;
            bufferedAttack = -1;

            if (SkillUnlockManager.Instance.IsUnlocked(next))
            {
                if (next == 2 && IsAttack2OnCooldown) yield break;
                StartCoroutine(ExecuteAttack(next));
            }
        }
    }

    // ───────────────────────────────────────────
    // W 쿨타임 루틴
    // UI 연동을 위해 Attack2CooldownRemaining 매 프레임 갱신
    // ───────────────────────────────────────────
    private IEnumerator Attack2CooldownRoutine()
    {
        IsAttack2OnCooldown = true;
        Attack2CooldownRemaining = cooldownTime2;

        while (Attack2CooldownRemaining > 0f)
        {
            Attack2CooldownRemaining -= Time.deltaTime;
            yield return null;
        }

        Attack2CooldownRemaining = 0f;
        IsAttack2OnCooldown = false;
    }

    // ───────────────────────────────────────────
    // W: 원거리 Slash 투사체 생성
    // hitBox2Transform 위치에서 플레이어 방향으로 발사
    // ───────────────────────────────────────────
    private void SpawnSlashProjectile()
    {
        if (slashProjectilePrefab == null)
        {
            Debug.LogError("[SwordAttack] slashProjectilePrefab이 연결되지 않았습니다.");
            return;
        }

        // 생성 위치: hitBox2Transform (flipX 방향 반영된 상태)
        Vector3 spawnPos = hitBox2Transform != null
            ? hitBox2Transform.position
            : transform.position;

        GameObject obj = Instantiate(slashProjectilePrefab, spawnPos, Quaternion.identity);
        SlashProjectile slash = obj.GetComponent<SlashProjectile>();

        if (slash != null)
        {
            float dir = spriteRenderer.flipX ? -1f : 1f;
            slash.Initialize(dir, damage2);
        }
    }

    // ───────────────────────────────────────────
    // HitBox 방향 적용 (Q 근접 공격용)
    // ───────────────────────────────────────────
    private void ApplyHitBoxDirection()
    {
        if (hitBox1 == null) return;

        bool facingLeft = spriteRenderer.flipX;
        Vector3 pos = hitBox1.transform.localPosition;
        pos.x = facingLeft ? -hitBoxDefaultOffsetX[0] : hitBoxDefaultOffsetX[0];
        hitBox1.transform.localPosition = pos;

        // hitBox2Transform도 방향 반영 (투사체 생성 위치)
        if (hitBox2Transform != null)
        {
            Vector3 pos2 = hitBox2Transform.localPosition;
            pos2.x = facingLeft ? -Mathf.Abs(pos2.x) : Mathf.Abs(pos2.x);
            hitBox2Transform.localPosition = pos2;
        }
    }

    // ───────────────────────────────────────────
    // HitBox 제어 (Q 근접 공격용)
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

    // ───────────────────────────────────────────
    // 유틸리티
    // ───────────────────────────────────────────
    private float GetDuration(int index) => index switch
    {
        1 => attackDuration1,
        2 => attackDuration2,
        _ => 0.4f
    };

    private int GetDamage(int index) => index switch
    {
        1 => damage1,
        2 => damage2,
        _ => 10
    };

    private Collider2D GetHitBox(int index) => index switch
    {
        1 => hitBox1,
        _ => null
    };
}
