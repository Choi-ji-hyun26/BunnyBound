using System.Collections;
using UnityEngine;

/// <summary>
/// 슬라임 순찰 상태
///
/// [설계 원칙]
/// - ThinkRoutine : 방향/정지 결정 + jumpChance 확률로 점프 여부 결정
/// - JumpRoutine  : doJump 트리거 + 물리 점프, 착지까지 대기
/// - Update       : velocity 적용 + 엣지/벽 감지 (점프 중 스킵)
/// </summary>
public class SlimePatrolState : IEnemyState
{
    private Slime slime;
    private EnemyStateMachine stateMachine;

    private const float THINK_MIN_DELAY  = 2f;
    private const float THINK_MAX_DELAY  = 4f;
    private const float INITIAL_THINK_DELAY = 1f;

    // -1(왼쪽), 0(정지), 1(오른쪽)
    private int nextMove = 0;

    // 점프 중엔 엣지/벽 체크 및 velocity 덮어쓰기 스킵
    private bool isJumping = false;

    private Coroutine thinkCoroutine;

    public SlimePatrolState(Slime slime, EnemyStateMachine stateMachine)
    {
        this.slime = slime;
        this.stateMachine = stateMachine;
    }

    // ─────────────────────────────────────────
    // State Machine
    // ─────────────────────────────────────────

    public void Enter()
    {
        nextMove   = 0;
        isJumping  = false;
        thinkCoroutine = slime.StartCoroutine(ThinkRoutine(INITIAL_THINK_DELAY));
    }

    public void Update()
    {
        // 점프 중엔 x velocity 유지, y는 물리에 위임
        // 점프 아닐 때도 구조 동일 — 분기 없이 통일
        slime.Rigid.velocity = new Vector2(nextMove * slime.MoveSpeed, slime.Rigid.velocity.y);

        // 점프 중 또는 정지 중엔 엣지/벽 체크 스킵
        if (!isJumping && nextMove != 0)
            PlatformCheck();
    }

    public void Exit()
    {
        if (thinkCoroutine != null) slime.StopCoroutine(thinkCoroutine);

        isJumping = false;
        slime.Rigid.velocity = Vector2.zero;
    }

    // ─────────────────────────────────────────
    // Think — 방향 결정 + 점프 확률 판정
    // ─────────────────────────────────────────

    private IEnumerator ThinkRoutine(float initialDelay = 0f)
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // 방향 결정
            int prevMove = nextMove;
            nextMove = Random.Range(-1, 2); // -1, 0, 1

            if (nextMove != 0)
                slime.FlipByDirection(nextMove);

            // 점프 확률 판정
            // - 착지 상태일 때만 점프 가능
            // - 정지(0) 상태로 결정됐을 때는 점프 안 함 (정지 중 점프는 어색함)
            bool shouldJump = nextMove != 0
                           && slime.IsGrounded()
                           && Random.Range(0, 100) < slime.JumpChance;

            if (shouldJump)
            {
                // 점프 완료까지 ThinkRoutine 일시 중단
                yield return slime.StartCoroutine(JumpRoutine());
            }

            float nextThinkTime = Random.Range(THINK_MIN_DELAY, THINK_MAX_DELAY);
            yield return new WaitForSeconds(nextThinkTime);
        }
    }

    // ─────────────────────────────────────────
    // Jump
    // ─────────────────────────────────────────

    private IEnumerator JumpRoutine()
    {
        isJumping = true;

        // y velocity 초기화 후 점프력 적용
        slime.Rigid.velocity = new Vector2(slime.Rigid.velocity.x, 0f);
        slime.Rigid.AddForce(Vector2.up * slime.JumpForce, ForceMode2D.Impulse);

        // Animator 트리거 — Jump 클립 재생
        slime.Animator.SetTrigger("doJump");

        // 물리 반영 대기 (FixedUpdate 2회) 후 이륙 확인
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => !slime.IsGrounded()); // 이륙 확인

        // 착지 대기
        yield return new WaitUntil(() => slime.IsGrounded());

        // Animator는 Jump → Patrol Exit Time으로 자동 복귀
        isJumping = false;
    }

    // ─────────────────────────────────────────
    // Platform / Wall Check
    // ─────────────────────────────────────────

    private void PlatformCheck()
    {
        if (slime.IsFacingEdge(nextMove) || IsHittingWall())
            Turn();
    }

    private void Turn()
    {
        nextMove *= -1;
        slime.FlipByDirection(nextMove);

        // ThinkRoutine 재시작 — 전환 직후 진동 방지
        if (thinkCoroutine != null) slime.StopCoroutine(thinkCoroutine);
        thinkCoroutine = slime.StartCoroutine(ThinkRoutine(0f));
    }

    private bool IsHittingWall()
    {
        float   checkDistance = slime.BoxCollider.size.x * 0.5f + 0.1f;
        Vector2 origin        = (Vector2)slime.transform.position + slime.BoxCollider.offset;
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * nextMove,
            checkDistance,
            slime.GroundLayer
        );
        return hit.collider != null;
    }
}
