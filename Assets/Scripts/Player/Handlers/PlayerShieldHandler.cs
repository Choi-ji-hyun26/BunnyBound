using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 검사 쉴드 시스템 (페링 방식)
/// - Shift 입력(단발) → Shield 클립 재생 + shieldDuration 동안 IsShielding 유지
/// - 쉴드 활성 구간: ShieldHitBox ON, 피격 무시, 이동 불가
/// - 적 접촉 시 이동형: 넉백 / 고정형: IsAttacking 중일 때만 stun
/// - 쿨타임 동안 재발동 불가
///
/// [애니메이션 파라미터]
/// doShield (Trigger): Shift 입력 시 발동 → Shield 클립 재생 → HasExitTime ON으로 자동 Idle 복귀
///
/// [추후 작업]
/// Animator 분리(RabbitSprite/KnightSprite 자식 오브젝트) 후
/// 애니메이션 이벤트(ShieldActivate/ShieldDeactivate)로 교체 권장
/// → shieldDuration 필드 제거 가능, 타이밍이 클립 speed에 자동으로 따라감
/// </summary>
public class PlayerShieldHandler : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float shieldDuration = 0.5f; // Shield 클립 길이에 맞게 설정
    [SerializeField] private float cooldownTime = 1.0f;
    public float CooldownTime => cooldownTime;

    [Header("Shield Reaction")]
    [SerializeField] private float knockbackSpeed = 8f;
    [SerializeField] private float stunDuration = 2f;

    [Header("Components")]
    [SerializeField] private ShieldHitBox shieldHitBox;
    [SerializeField] private PlayerCoordinator coordinator;
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerTransformHandler transformHandler;
    [SerializeField] private PlayerStateMachine stateMachine;

    public bool IsShielding { get; private set; } = false;
    public bool IsOnCooldown { get; private set; } = false;
    public float CooldownRemaining { get; private set; } = 0f;

    private Animator animator;
    private HashSet<EnemyBase> reactedEnemies = new HashSet<EnemyBase>();
    private Coroutine shieldCoroutine;

    private void Awake()
    {
        if (coordinator == null)
            Debug.LogError("[PlayerShieldHandler] coordinator가 Inspector에 연결되지 않았습니다.");
        if (input == null)
            Debug.LogError("[PlayerShieldHandler] input이 Inspector에 연결되지 않았습니다.");
        if (transformHandler == null)
            Debug.LogError("[PlayerShieldHandler] transformHandler가 Inspector에 연결되지 않았습니다.");
        if (stateMachine == null)
            Debug.LogError("[PlayerShieldHandler] stateMachine이 Inspector에 연결되지 않았습니다.");

        animator = coordinator.Animator;
    }

    private void Update()
    {
        if (transformHandler.currentType != CharacterType.Knight) return;
        if (IsOnCooldown || IsShielding) return;

        if (input.ShieldPressed)
            shieldCoroutine = StartCoroutine(ShieldRoutine());
    }

    // ───────────────────────────────────────────
    // 쉴드 발동 루틴
    // shieldDuration 동안 IsShielding 유지 후 자동 해제
    // Trigger는 Animator가 자동 리셋하므로 별도 해제 불필요
    // ───────────────────────────────────────────
    private IEnumerator ShieldRoutine()
    {
        IsShielding = true;
        reactedEnemies.Clear();
        stateMachine.CanMove = false;
        animator.SetTrigger("doShield");

        if (shieldHitBox != null)
            shieldHitBox.Activate();

        yield return new WaitForSeconds(shieldDuration);

        DeactivateShield();
        shieldCoroutine = StartCoroutine(CooldownRoutine());
    }

    private void DeactivateShield()
    {
        IsShielding = false;
        reactedEnemies.Clear();
        stateMachine.CanMove = true;

        if (shieldHitBox != null)
            shieldHitBox.Deactivate();
    }

    // ───────────────────────────────────────────
    // ShieldHitBox에서 호출 — 적 반응 처리
    // 이동형: 넉백 / 고정형: IsAttacking 중일 때만 stun
    // ───────────────────────────────────────────
    public void ReactToEnemy(EnemyBase enemy)
    {
        if (enemy == null) return;
        if (reactedEnemies.Contains(enemy)) return;

        if (enemy.IsMovingEnemy)
        {
            reactedEnemies.Add(enemy);
            float dirX = enemy.transform.position.x - transform.position.x;
            Vector2 knockbackDir = new Vector2(Mathf.Sign(dirX), 0f);
            enemy.TakeKnockback(knockbackDir, knockbackSpeed);
        }
        else
        {
            if (!enemy.IsAttacking) return;
            reactedEnemies.Add(enemy);
            enemy.Stun(stunDuration);
        }
    }

    // ───────────────────────────────────────────
    // 쿨타임
    // ───────────────────────────────────────────
    private IEnumerator CooldownRoutine()
    {
        IsOnCooldown = true;
        CooldownRemaining = cooldownTime;

        while (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
            yield return null;
        }

        CooldownRemaining = 0f;
        IsOnCooldown = false;
    }

    // ───────────────────────────────────────────
    // 스테이지 전환 시 리셋 — 진행 중인 코루틴 명시적 중단
    // ───────────────────────────────────────────
    private void OnEnable()
    {
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }

        DeactivateShield();
        IsOnCooldown = false;
    }
}
