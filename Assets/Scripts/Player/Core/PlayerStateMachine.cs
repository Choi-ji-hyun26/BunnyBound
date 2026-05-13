using System.Collections;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    // 컴포넌트 참조
    private PlayerCoordinator coordinator;
    public PlayerCoordinator Coordinator => coordinator;
    public bool CanMove { get; set; } = true;

    [Header("Movement Stats")]
    public float maxSpeed = 6f;
    public float firstJumpForce = 22f;
    public float doubleJumpForce = 18f;
    public int maxJumpCount = 2;
    public float defaultGravity = 4f;

    [HideInInspector] public int currentJumpCount = 0; // State들이 공유하는 데이터

    [Header("Ground")]
    [SerializeField] private ContactFilter2D groundFilter; // Layer : Platform, OneWayPlatform
    [SerializeField] private float groundCheckDistance = 0.1f;
    private RaycastHit2D[] groundHits = new RaycastHit2D[5]; // 결과 저장용 배열
    public bool IsGroundedCached { get; private set; }

    [Header("Ladder")]
    [SerializeField] private float climbSpeed = 4f;

    // 사다리 상태 (LadderZone에서 제어)
    public bool IsOnLadder { get; private set; } = false;
    public float ClimbSpeed => climbSpeed;
    public bool HasLadder() => IsOnLadder;
    
    // State
    public PlayerState CurrentState { get; private set; }
    public IdleState IdleState { get; private set; }
    public WalkState WalkState { get; private set; }
    public JumpState JumpState { get; private set; }
    public FallState FallState { get; private set; }
    public ClimbState ClimbState { get; private set; }

    // Input system
    private PlayerInputHandler input;
    public PlayerInputHandler Input => input;

    private PlayerTransformHandler transformHandler;

    private int playerLayerBit;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if(coordinator == null)
        {
            Debug.LogError("PlayerStateMachine requires PlayerCoordinator component!");
            return;
        }   

        input = GetComponent<PlayerInputHandler>();
        if (input == null)
        {
            Debug.LogError("PlayerInputHandler required!");
        }

        transformHandler = GetComponent<PlayerTransformHandler>();
        if (transformHandler == null)
            Debug.LogError("PlayerTransformHandler required!");

        // 모든 상태 인스턴스 초기화 (자신(stateMachine)을 인자로 넘김)
        IdleState = new IdleState(this);
        WalkState = new WalkState(this);
        JumpState = new JumpState(this);
        FallState = new FallState(this);
        ClimbState = new ClimbState(this);

        playerLayerBit = 1 << LayerMask.NameToLayer("Player");
    }

    private void Start()
    {
        ChangeState(IdleState);
    }

    private void Update()
    {
        // 변신 입력 처리
        if (input.TransformPressed && CanMove)
            transformHandler.TransformCharacter();

        CurrentState?.UpdateState();
    }

    private void FixedUpdate()
    {
        IsGroundedCached = IsGrounded();
        CurrentState?.FixedUpdateState();
    }

    // 상태 전환 메서드
    public void ChangeState(PlayerState newState)
    {
        // 기존 상태 종료
        CurrentState?.ExitState();

        // 새 상태 설정 및 진입
        CurrentState = newState;
        CurrentState.EnterState();
    }

    public bool IsGrounded()
    {
        int hitCount = coordinator.Rigid.Cast(
            Vector2.down,
            groundFilter,
            groundHits,
            groundCheckDistance
        );

        for (int i = 0; i < hitCount; i++)
        {
            Vector2 normal = groundHits[i].normal;
            // 위쪽을 향한 surface만 ground로 인정
            if (normal.y > 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// LadderZone에서 호출 — 사다리 진입/퇴장 시
    /// </summary>
    public void SetOnLadder(bool value)
    {
        IsOnLadder = value;

        if (!value && CurrentState is ClimbState)
        {
            if (IsGroundedCached)
                ChangeState(IdleState);
            else
                ChangeState(FallState);
        }
    }
    
    public void VelocityZero()
    {
        if(coordinator.Rigid != null)
        {
            coordinator.Rigid.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 사다리 진입 시 OneWayPlatform을 잠시 통과 허용
    /// PlatformEffector2D.colliderMask에서 Player 레이어를 잠시 제거
    /// </summary>
    public void StartLadderDrop()
    {
        StartCoroutine(LadderDropRoutine());
    }

    private IEnumerator LadderDropRoutine()
    {
        PlatformEffector2D[] effectors = FindObjectsOfType<PlatformEffector2D>();

        // OneWayPlatform 통과 허용
        foreach (var effector in effectors)
            effector.colliderMask &= ~playerLayerBit;

        yield return new WaitForSeconds(0.4f);

        // 복원
        foreach (var effector in effectors)
            effector.colliderMask |= playerLayerBit;
    }
}
