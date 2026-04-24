using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    protected EnemyStateMachine stateMachine;

    // 체력
    [SerializeField] protected int maxHp = 1;
    protected int currentHp;

    // 디버그 UI용 프로퍼티
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    [SerializeField] private bool defaultFacingLeft = true;

    protected Rigidbody2D rigid;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;

    public Rigidbody2D Rigid => rigid;
    public Animator Animator => animator;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public BoxCollider2D BoxCollider => boxCollider;

    protected virtual void Awake()
    {
        stateMachine = new EnemyStateMachine();
        currentHp = maxHp;

        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Update()
    {
        stateMachine.Update();
    }

    public void FlipByDirection(float directionX)
    {
        if(Mathf.Abs(directionX) < 0.01f)
            return;
        bool movingLeft = directionX  < 0f;
        spriteRenderer.flipX = defaultFacingLeft ? !movingLeft : movingLeft;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHp -= amount;

        if(currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        stateMachine.ChangeState(new EnemyDeathState(this));
    }
}
