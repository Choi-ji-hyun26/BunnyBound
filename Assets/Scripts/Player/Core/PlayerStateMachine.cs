using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerCoordinator coordinator;
    public PlayerCoordinator Coordinator => coordinator;
    public bool CanMove { get; set; } = true;

    [Header("Movement Stats")]
    public float maxSpeed = 6f;
    public float firstJumpForce = 22f;
    public float doubleJumpForce = 18f;
    public int maxJumpCount = 2;
    public float defaultGravity = 4f;

    [HideInInspector] public int currentJumpCount = 0;

    [Header("Ground")]
    [SerializeField] private ContactFilter2D groundFilter;
    [SerializeField] private float groundCheckDistance = 0.1f;
    private RaycastHit2D[] groundHits = new RaycastHit2D[5];
    public bool IsGroundedCached { get; private set; }

    // State
    public PlayerState CurrentState { get; private set; }
    public IdleState IdleState { get; private set; }
    public WalkState WalkState { get; private set; }
    public JumpState JumpState { get; private set; }
    public FallState FallState { get; private set; }

    private PlayerInputHandler input;
    public PlayerInputHandler Input => input;

    private PlayerTransformHandler transformHandler;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if (coordinator == null)
        {
            Debug.LogError("PlayerStateMachine requires PlayerCoordinator component!");
            return;
        }

        input = GetComponent<PlayerInputHandler>();
        if (input == null)
            Debug.LogError("PlayerInputHandler required!");

        transformHandler = GetComponent<PlayerTransformHandler>();
        if (transformHandler == null)
            Debug.LogError("PlayerTransformHandler required!");

        IdleState  = new IdleState(this);
        WalkState  = new WalkState(this);
        JumpState  = new JumpState(this);
        FallState  = new FallState(this);
    }

    private void Start()
    {
        ChangeState(IdleState);
    }

    private void Update()
    {
        if (input.TransformPressed && CanMove)
            transformHandler.TransformCharacter();

        CurrentState?.UpdateState();
    }

    private void FixedUpdate()
    {
        IsGroundedCached = IsGrounded();
        CurrentState?.FixedUpdateState();
    }

    public void ChangeState(PlayerState newState)
    {
        CurrentState?.ExitState();
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
            if (groundHits[i].normal.y > 0.5f)
                return true;
        }
        return false;
    }

    public void VelocityZero()
    {
        if (coordinator.Rigid != null)
            coordinator.Rigid.velocity = Vector2.zero;
    }
}
