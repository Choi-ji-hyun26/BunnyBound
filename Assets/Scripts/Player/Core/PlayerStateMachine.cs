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
    public float firstJumpForce = 20f;
    public float doubleJumpForce = 16f;
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

    private Ladder currentLadder;
    private Collider2D ignoredTopPlatform;

    public Ladder CurrentLadder => currentLadder;
    public bool HasLadder() => currentLadder != null;
    public float ClimbSpeed => climbSpeed;
    
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

    public void SetCurrentLadder(Ladder ladder)
    {
        currentLadder = ladder;
    }

    public void ClearCurrentLadder(Ladder ladder)
    {
        if (currentLadder == ladder)
            currentLadder = null;
    }

    public void SnapToLadderCenter()
    {
        if (currentLadder == null)
            return;

        Vector2 pos = coordinator.Rigid.position;
        pos.x = currentLadder.CenterX;
        coordinator.Rigid.position = pos;
    }

    public void IgnoreLadderTopPlatform()
    {
        if (currentLadder == null || currentLadder.TopExitPlatform == null)
            return;

        if (ignoredTopPlatform != null)
            return;

        ignoredTopPlatform = currentLadder.TopExitPlatform;
        Physics2D.IgnoreCollision(coordinator.BoxCollider, ignoredTopPlatform, true);
    }

    public void RestoreLadderTopPlatform()
    {
        if (ignoredTopPlatform == null)
            return;

        Physics2D.IgnoreCollision(coordinator.BoxCollider, ignoredTopPlatform, false);
        ignoredTopPlatform = null;
    }

    public void MoveToLadderTopMount()
    {
        if (currentLadder == null || currentLadder.TopMountPoint == null)
            return;

        coordinator.Rigid.position = currentLadder.TopMountPoint.position;
    }

    public void MoveToLadderBottomMount()
    {
        if (currentLadder == null || currentLadder.BottomMountPoint == null)
            return;

        coordinator.Rigid.position = currentLadder.BottomMountPoint.position;
    }
    
    public void VelocityZero()
    {
        if(coordinator.Rigid != null)
        {
            coordinator.Rigid.velocity = Vector2.zero;
        }
    }
}