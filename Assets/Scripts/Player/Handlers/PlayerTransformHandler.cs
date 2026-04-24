using UnityEngine;

public enum CharacterType { Rabbit, Knight }

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

    [Header("Knight Stats")]
    [SerializeField] private float knightMoveSpeed = 3.5f;
    [SerializeField] private float knightFirstJumpForce = 14f;
    [SerializeField] private float knightDoubleJumpForce = 11f;
    [SerializeField] private Vector2 knightColliderSize = new Vector2(0.9f, 1.4f);
    [SerializeField] private Vector2 knightColliderOffset = new Vector2(0f, 0.2f);

    [Header("Animators")]
    [SerializeField] private RuntimeAnimatorController rabbitAnimator;
    [SerializeField] private RuntimeAnimatorController knightAnimator;

    [Header("HP UI")]
    [SerializeField] private GameObject rabbitHPUI;      // 토끼 HP 칸 이미지 루트
    [SerializeField] private GameObject knightHPUI;  // 검사 HP 슬라이더 루트

    [Header("Transform Control")]
    public bool CanTransform = true; // false 시 변신 불가 (변신 불가 구간에서 외부 제어)

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
        // 시작 시 토끼 스탯 + UI 초기화
        ApplyStats(CharacterType.Rabbit);
        ApplyHPUI(CharacterType.Rabbit);
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
        ApplyHPUI(CharacterType.Rabbit);
        ApplyPhysicsLayer(CharacterType.Rabbit);
    }

    /// PlayerInputHandler에서 호출, 현재 타입의 반대로 변신
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

        // 6. HP UI 전환
        ApplyHPUI(next);

        // 7. 레이어 충돌 설정
        // 검사: Player ↔ Enemy 물리 충돌 OFF (HitBox Trigger로만 처리)
        // 토끼: Player ↔ Enemy 물리 충돌 ON  (점프 공격 OnCollisionEnter2D 사용)
        ApplyPhysicsLayer(next);

        // 8. 변신 후 Idle로 리셋 (애니메이션 꼬임 방지)
        stateMachine.ChangeState(stateMachine.IdleState);

        Debug.Log($"[Transform] {next}로 변신 완료");
    }

    // ───────────────────────────────────────────
    // HP 환산
    // 검사 HP를 savedKnightHP에 저장해두고
    // 검사로 복귀 시 저장값 기준으로 토끼 피격 반영
    // ───────────────────────────────────────────
    private void ConvertHP(CharacterType from, CharacterType to)
    {
        if (from == CharacterType.Rabbit && to == CharacterType.Knight)
        {
            // 토끼 → 검사
            // savedKnightHP 저장 시점의 토끼 칸수를 Clamp(1~3)으로 구하고
            // 현재 토끼 칸수와 비교해 잃은 만큼 검사 HP 차감
            // CeilToInt(100/33.3) = 4 버그 방지를 위해 Clamp(1,3) 처리
            int rabbitHPAtSave = Mathf.Clamp(Mathf.CeilToInt(stats.savedKnightHP / 33.3f), 1, 3);
            int rabbitHPLost = rabbitHPAtSave - stats.health;
            float restoredHP = stats.savedKnightHP - (rabbitHPLost * 33.3f);
            stats.SetKnightHP(Mathf.Max(restoredHP, 1f));
        }
        else if (from == CharacterType.Knight && to == CharacterType.Rabbit)
        {
            // 검사 → 토끼: 현재 검사 HP 저장 후 칸 수로 환산
            stats.savedKnightHP = stats.knightHP;

            int converted = Mathf.CeilToInt(stats.knightHP / 33.3f);
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
            stateMachine.maxSpeed = knightMoveSpeed;
            stateMachine.firstJumpForce = knightFirstJumpForce;
            stateMachine.doubleJumpForce = knightDoubleJumpForce;
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
    // 레이어 충돌 설정
    // 검사: Player ↔ Enemy 물리 충돌 OFF
    // 토끼: Player ↔ Enemy 물리 충돌 ON
    // ───────────────────────────────────────────
    private void ApplyPhysicsLayer(CharacterType type)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer  = LayerMask.NameToLayer("Enemy");

        if (playerLayer < 0 || enemyLayer < 0)
        {
            Debug.LogWarning("[Transform] Player 또는 Enemy 레이어가 존재하지 않습니다. 레이어 이름을 확인해주세요.");
            return;
        }

        bool ignore = (type == CharacterType.Knight);
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, ignore);

        Debug.Log($"[Transform] Player \u2194 Enemy \ucda9\ub3cc {(ignore ? "OFF" : "ON")}");
    }

    // HP UI 
    private void ApplyHPUI(CharacterType type)
    {
        bool isRabbit = (type == CharacterType.Rabbit);

        if (rabbitHPUI != null)   rabbitHPUI.SetActive(isRabbit);
        if (knightHPUI != null) knightHPUI.SetActive(!isRabbit);
    }
}
