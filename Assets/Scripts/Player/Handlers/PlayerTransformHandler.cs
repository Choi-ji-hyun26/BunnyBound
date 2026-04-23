using UnityEngine;

public enum CharacterType { Rabbit, Swordsman }

/*
역할 
토끼 ↔ 검사 변신 시스템
- 스탯 교체 (이동속도, 점프력, HP)
- 애니메이터 교체
- 콜라이더 크기 교체
 추후 쿨타임 추가 시 TransformCharacter() 앞에 조건만 추가하면 됨
*/
public class PlayerTransformHandler : MonoBehaviour
{
    [Header("Character Type")]
    public CharacterType currentType = CharacterType.Rabbit;

    [Header("Rabbit Stats")]
    [SerializeField] private float rabbitMoveSpeed = 6f;
    [SerializeField] private float rabbitFirstJumpForce = 20f;
    [SerializeField] private float rabbitDoubleJumpForce = 16f;
    [SerializeField] private Vector2 rabbitColliderSize = new Vector2(0.8f, 1f);
    [SerializeField] private Vector2 rabbitColliderOffset = new Vector2(0f, 0f);

    [Header("Swordsman Stats")]
    [SerializeField] private float swordsmanMoveSpeed = 3.5f;
    [SerializeField] private float swordsmanFirstJumpForce = 14f;
    [SerializeField] private float swordsmanDoubleJumpForce = 11f;
    [SerializeField] private Vector2 swordsmanColliderSize = new Vector2(0.9f, 1.4f);
    [SerializeField] private Vector2 swordsmanColliderOffset = new Vector2(0f, 0.2f);

    [Header("Animators")]
    [SerializeField] private RuntimeAnimatorController rabbitAnimator;
    [SerializeField] private RuntimeAnimatorController swordsmanAnimator;

    private PlayerCoordinator coordinator;
    private PlayerStateMachine stateMachine;
    private PlayerStats stats;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        stateMachine = GetComponent<PlayerStateMachine>();
        stats = GetComponent<PlayerStats>();

        if (coordinator == null || stateMachine == null || stats == null)
            Debug.LogError("PlayerTransformHandler: 필수 컴포넌트가 없습니다!");
    }

    private void Start()
    {
        // 시작 시 토끼 스탯으로 초기화
        ApplyStats(CharacterType.Rabbit);
    }

    /// PlayerInputHandler에서 호출, 현재 타입의 반대로 변신
    public void TransformCharacter()
    {
        CharacterType next = (currentType == CharacterType.Rabbit)
            ? CharacterType.Swordsman
            : CharacterType.Rabbit;

        SwitchTo(next);
    }

    private void SwitchTo(CharacterType next)
    {
        // 1. HP 환산
        ConvertHP(currentType, next);

        // 2. 스탯 교체
        ApplyStats(next);

        // 3. 애니메이터 교체
        ApplyAnimator(next);

        // 4. 콜라이더 크기 교체
        ApplyCollider(next);

        // 5. 현재 상태 업데이트
        currentType = next;

        // 6. 변신 후 Idle로 리셋 (애니메이션 꼬임 방지)
        stateMachine.ChangeState(stateMachine.IdleState);

        Debug.Log($"[Transform] {next}로 변신 완료");
    }

    // ───────────────────────────────────────────
    // HP 환산
    // 토끼 HP 1칸 = 검사 HP 33.3%
    // ───────────────────────────────────────────
    private void ConvertHP(CharacterType from, CharacterType to)
    {
        if (from == CharacterType.Rabbit && to == CharacterType.Swordsman)
        {
            // 토끼 → 검사: 칸 수를 % 게이지로 환산
            stats.SetSwordsmanHP(stats.health * 33.3f);
        }
        else if (from == CharacterType.Swordsman && to == CharacterType.Rabbit)
        {
            // 검사 → 토끼: % 게이지를 칸 수로 환산 (올림)
            int converted = Mathf.CeilToInt(stats.swordsmanHP / 33.3f);
            stats.health = Mathf.Clamp(converted, 1, 3);
            stats.RefreshRabbitHPUI();
        }
    }

    // ───────────────────────────────────────────
    // 스탯 적용
    // ───────────────────────────────────────────
    private void ApplyStats(CharacterType type)
    {
        if (type == CharacterType.Rabbit)
        {
            stateMachine.maxSpeed = rabbitMoveSpeed;
            stateMachine.firstJumpForce = rabbitFirstJumpForce;
            stateMachine.doubleJumpForce = rabbitDoubleJumpForce;
        }
        else
        {
            stateMachine.maxSpeed = swordsmanMoveSpeed;
            stateMachine.firstJumpForce = swordsmanFirstJumpForce;
            stateMachine.doubleJumpForce = swordsmanDoubleJumpForce;
        }
    }

    // ───────────────────────────────────────────
    // 애니메이터 교체
    // ───────────────────────────────────────────
    private void ApplyAnimator(CharacterType type)
    {
        var anim = coordinator.Animator;
        if (anim == null) return;

        RuntimeAnimatorController target = (type == CharacterType.Rabbit)
            ? rabbitAnimator
            : swordsmanAnimator;

        if (target != null)
            anim.runtimeAnimatorController = target;
        else
            Debug.LogWarning($"[Transform] {type} Animator가 Inspector에 연결되지 않았습니다.");
    }

    // ───────────────────────────────────────────
    // 콜라이더 크기 교체
    // ───────────────────────────────────────────
    private void ApplyCollider(CharacterType type)
    {
        var col = coordinator.BoxCollider;
        if (col == null) return;

        if (type == CharacterType.Rabbit)
        {
            col.size = rabbitColliderSize;
            col.offset = rabbitColliderOffset;
        }
        else
        {
            col.size = swordsmanColliderSize;
            col.offset = swordsmanColliderOffset;
        }
    }
}
