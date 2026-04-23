using System.Collections;
using UnityEngine;

public class SlugPatrolState : IEnemyState
{
    private Slug slug;
    private EnemyStateMachine stateMachine;

    // 상수화
    private const int MOVE_LEFT = -1;
    private const int MOVE_IDLE = 0;
    private const int MOVE_RIGHT = 1;

    private const float THINK_MIN_DELAY = 2f;
    private const float THINK_MAX_DELAY = 4f;
    private const float INITIAL_THINK_DELAY = 2f;

    private int nextMove;
    private Coroutine thinkCoroutine;

    public SlugPatrolState(Slug slug, EnemyStateMachine stateMachine)
    {
        this.slug = slug;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        thinkCoroutine = slug.StartCoroutine(ThinkRoutine(INITIAL_THINK_DELAY));
    }

    public void Update()
    {
        Move();
        PlatformCheck();
    }

    public void Exit()
    {
        if(thinkCoroutine != null)
            slug.StopCoroutine(thinkCoroutine);
    }

    private void Move()
    {
        slug.Rigid.velocity = new Vector2(nextMove, slug.Rigid.velocity.y);        
    }

    private void PlatformCheck()
    {
        float detectionOffset = slug.BoxCollider.size.x * 0.5f;

        Vector2 frontVector = new Vector2(
            slug.Rigid.position.x + (nextMove * detectionOffset), 
            slug.Rigid.position.y
        );

        RaycastHit2D rayHit = Physics2D.Raycast(
            frontVector, 
            Vector3.down, 
            1.1f, 
            slug.GroundLayer
        );

        if(rayHit.collider == null) 
        {
            Turn();
        }
    }
    private IEnumerator ThinkRoutine(float initialDelay = 0f)
    {
        // 초기 딜레이 : 스폰 직후 바로 이동하지 않고 잠시 대기
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // 랜덤 이동 결정, -1(왼), 0(idle), 1(오) 중 선택
            nextMove = Random.Range(MOVE_LEFT, MOVE_RIGHT+1); 

            // Sprite 방향 전환
            if (nextMove != MOVE_IDLE)
                slug.FlipByDirection(nextMove);
                
            // 다음 Think까지 딜레이
            float nextThinkTime = Random.Range(THINK_MIN_DELAY, THINK_MAX_DELAY);
            yield return new WaitForSeconds(nextThinkTime);
        }
    }

    private void Turn()
    {
        nextMove *= -1; // 이동 방향 반전

        slug.FlipByDirection(nextMove);
        // Coroutine 재시작, 플랫폼 끝 감지 -> 기존 ThinkRoutine 중지 -> 일정 딜레이 후 새로 시작
        if (thinkCoroutine != null)
            slug.StopCoroutine(thinkCoroutine);

        thinkCoroutine = slug.StartCoroutine(ThinkRoutine(0f));
    }
}
