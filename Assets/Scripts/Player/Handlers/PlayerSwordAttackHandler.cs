using System.Collections;
using UnityEngine;

/// <summary>
/// 검사 전용 공격 시스템
/// - Q: 근접 slash (hitBox1)
/// - W: 원거리 slash 투사체 (SlashProjectile)
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

    [Header("데미지 — WeaponUpgradeConfig 공유 에셋 참조 (WeaponUpgradeManager와 동일한 에셋 연결)")]
    [SerializeField] private WeaponUpgradeConfig damageConfig;

    private int damage1; // Q — damageConfig.GetDamage1(tier), ApplyWeaponUpgrade()에서 갱신
    private int damage2; // W — damageConfig.GetDamage2(tier), ApplyWeaponUpgrade()에서 갱신

    [Header("W 쿨타임")]
    [SerializeField] private float cooldownTime2 = 2.5f;
    public float CooldownTime2 => cooldownTime2;
    public bool IsAttack2OnCooldown { get; private set; } = false;
    public float Attack2CooldownRemaining { get; private set; } = 0f;

    // Q 입력 피드백
    public float AttackDuration1 => attackDuration1;
    public float Attack1ActiveRemaining { get; private set; } = 0f;

    [Header("공격 지속 시간")]
    [SerializeField] private float attackDuration1 = 0.4f;
    [SerializeField] private float attackDuration2 = 0.4f;

    [Header("HitBox 활성화 타이밍 (0~1, 애니메이션 진행률 기준)")]
    [SerializeField] private float hitBoxStartRatio = 0.2f;
    [SerializeField] private float hitBoxEndRatio   = 0.7f;

    public bool IsAttacking { get; private set; } = false;
    private int bufferedAttack = -1;

    private float[] hitBoxDefaultOffsetX = new float[1]; // hitBox1만 관리

    // OverlapBoxNonAlloc용 결과 배열 — 매 공격마다 GC 할당 제거
    private readonly Collider2D[] overlapResults = new Collider2D[16];

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

        // WeaponUpgradeManager.Instance가 아직 없을 수 있으므로(여러 Awake 순서 미보장)
        // GameProgress를 직접 읽는 폴백이 ApplyWeaponUpgrade() 내부에 있음
        ApplyWeaponUpgrade();
    }

    private void Start()
    {
        // 모든 Awake가 끝난 시점이라 Instance가 확실하게 세팅된 상태
        if (WeaponUpgradeManager.Instance != null)
            WeaponUpgradeManager.Instance.OnWeaponUpgraded += OnWeaponUpgraded;
    }

    private void OnDestroy()
    {
        if (WeaponUpgradeManager.Instance != null)
            WeaponUpgradeManager.Instance.OnWeaponUpgraded -= OnWeaponUpgraded;
    }

    // 게임 중 강화 시 실시간 데미지 반영 (Game 씬 강화 창 지원)
    private void OnWeaponUpgraded(int newTier) => ApplyWeaponUpgrade();

    private void ApplyWeaponUpgrade()
    {
        if (damageConfig == null)
        {
            Debug.LogError("[PlayerSwordAttackHandler] damageConfig가 연결되지 않았습니다.");
            return;
        }

        int tier = WeaponUpgradeManager.Instance != null
            ? WeaponUpgradeManager.Instance.CurrentTier
            : GameProgress.GetWeaponUpgradeTier();

        damage1 = damageConfig.GetDamage1(tier);
        damage2 = damageConfig.GetDamage2(tier);
    }

    private void Update()
    {
        if (transformHandler.currentType != CharacterType.Knight) return;
        HandleAttackInput();

        // Q 입력 피드백 진행
        if (Attack1ActiveRemaining > 0f)
            Attack1ActiveRemaining = Mathf.Max(0f, Attack1ActiveRemaining - Time.deltaTime);
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

        PlayAttackSound(attackIndex);

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

            // Q 입력 피드백 시작
            Attack1ActiveRemaining = attackDuration1;

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

    private void PlayAttackSound(int attackIndex)
    {
        switch (attackIndex)
        {
            case 1:
                SoundManager.Instance.PlaySound(SoundType.AttackQ);
                break;

            case 2:
                SoundManager.Instance.PlaySound(SoundType.AttackW);
                break;
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

        // OverlapBoxNonAlloc — 매 공격마다 새 배열 할당 제거 (GC 절감)
        int hitCount = Physics2D.OverlapBoxNonAlloc(center, size, angle, overlapResults);
        for (int i = 0; i < hitCount; i++)
        {
            if (!overlapResults[i].CompareTag("Breakable")) continue;
            IBreakable breakable = overlapResults[i].GetComponent<IBreakable>();
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
