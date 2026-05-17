using UnityEngine;

public enum CharacterType { Rabbit, Knight }

/// <summary>
/// 토끼 ↔ 검사 변신 시스템
/// - 스탯 교체 (이동속도, 점프력)
/// - 애니메이터 교체
/// - 콜라이더 크기 교체
/// 추후 쿨타임 추가 시 TransformCharacter() 앞에 조건만 추가하면 됨
/// </summary>
public class PlayerTransformHandler : MonoBehaviour
{
    [Header("Character Type")]
    public CharacterType currentType = CharacterType.Rabbit;

    [Header("Rabbit Stats")]
    [SerializeField] private float rabbitMoveSpeed = 6f;
    [SerializeField] private float rabbitFirstJumpForce = 22f;
    [SerializeField] private float rabbitDoubleJumpForce = 18f;
    [SerializeField] private Vector2 rabbitColliderSize = new Vector2(0.8f, 1f);
    [SerializeField] private Vector2 rabbitColliderOffset = new Vector2(0f, 0f);

    [Header("Knight Stats")]
    [SerializeField] private float knightMoveSpeed = 3.5f;
    [SerializeField] private float knightFirstJumpForce = 15f;
    [SerializeField] private float knightDoubleJumpForce = 12f;
    [SerializeField] private Vector2 knightColliderSize = new Vector2(0.9f, 1.4f);
    [SerializeField] private Vector2 knightColliderOffset = new Vector2(0f, 0.2f);

    [Header("Sprite Scale")]
    [SerializeField] private Transform spriteObject;
    [SerializeField] private Vector3 rabbitSpriteScale = new Vector3(0.7f, 0.7f, 1f);
    [SerializeField] private Vector3 knightSpriteScale = new Vector3(1f, 1f, 1f);

    [Header("Animators")]
    [SerializeField] private RuntimeAnimatorController rabbitAnimator;
    [SerializeField] private RuntimeAnimatorController knightAnimator;

    [Header("Skill Slot UI")]
    [SerializeField] private GameObject skillSlotContainer;

    [Header("Transform Control")]
    public bool CanTransform = true;

    private PlayerCoordinator coordinator;
    private PlayerStateMachine stateMachine;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        stateMachine = GetComponent<PlayerStateMachine>();

        if (coordinator == null || stateMachine == null)
            Debug.LogError("PlayerTransformHandler: 필수 컴포넌트가 없습니다!");
    }

    private void Start()
    {
        ApplyStats(CharacterType.Rabbit);
        ApplySpriteScale(CharacterType.Rabbit);

         // 토끼 상태로 시작 — 스킬 슬롯 UI 초기 비활성화
        if (skillSlotContainer != null)
            skillSlotContainer.SetActive(false);
    }

    /// <summary>
    /// 스테이지 전환 시 토끼 상태로 완전 리셋 (PlayerStats.ResetForNextStage에서 호출)
    /// </summary>
    public void ResetToRabbit()
    {
        currentType = CharacterType.Rabbit;
        ApplyStats(CharacterType.Rabbit);
        ApplyAnimator(CharacterType.Rabbit);
        ApplyCollider(CharacterType.Rabbit);
        ApplySpriteScale(CharacterType.Rabbit);
        if (skillSlotContainer != null)
            skillSlotContainer.SetActive(false);
    }

    /// <summary>
    /// PlayerInputHandler에서 호출, 현재 타입의 반대로 변신
    /// </summary>
    public void TransformCharacter()
    {
        if (!CanTransform)
        {
            Debug.Log("[Transform] 이 구간에서는 변신할 수 없습니다.");
            return;
        }

        CharacterType next = (currentType == CharacterType.Rabbit)
            ? CharacterType.Knight
            : CharacterType.Rabbit;

        SwitchTo(next);
    }

    private void SwitchTo(CharacterType next)
    {
        ApplyStats(next);
        ApplyAnimator(next);
        ApplyCollider(next);

        currentType = next;

        ApplySpriteScale(next);

        // 스킬 슬롯 UI — 검사일 때만 활성화 (데스크탑 전용)
        if (skillSlotContainer != null && !Application.isMobilePlatform)
            skillSlotContainer.SetActive(next == CharacterType.Knight);

        // 변신 후 Idle로 리셋 (애니메이션 꼬임 방지)
        stateMachine.ChangeState(stateMachine.IdleState);

        Debug.Log($"[Transform] {next}로 변신 완료");
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
            stateMachine.maxSpeed = knightMoveSpeed;
            stateMachine.firstJumpForce = knightFirstJumpForce;
            stateMachine.doubleJumpForce = knightDoubleJumpForce;
        }
    }

    // ───────────────────────────────────────────
    // 애니메이터 교체
    // ───────────────────────────────────────────
    public void ApplyAnimator(CharacterType type)
    {
        var anim = coordinator.Animator;
        if (anim == null) return;

        RuntimeAnimatorController target = (type == CharacterType.Rabbit)
            ? rabbitAnimator
            : knightAnimator;

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
            col.size = knightColliderSize;
            col.offset = knightColliderOffset;
        }
    }

    // ───────────────────────────────────────────
    // 스프라이트 스케일 교체
    // ───────────────────────────────────────────
    private void ApplySpriteScale(CharacterType type)
    {
        if (spriteObject == null)
        {
            Debug.LogWarning("[Transform] spriteObject가 Inspector에 연결되지 않았습니다.");
            return;
        }

        spriteObject.localScale = (type == CharacterType.Rabbit)
            ? rabbitSpriteScale
            : knightSpriteScale;
    }
}
