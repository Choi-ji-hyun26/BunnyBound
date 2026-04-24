using UnityEngine;

/*
역할
1. 검사 공격 HitBox
2. PlayerSwordAttackHandler가 활성화/비활성화 제어
3. 적 또는 파괴 가능 오브젝트 감지 시 데미지 전달
*/
public class SwordHitBox : MonoBehaviour
{
    private int damage = 10;
    private Collider2D playerCollider;

    private void Awake()
    {
        // 플레이어 콜라이더 참조 (자기 자신 충돌 방지)
        playerCollider = GetComponentInParent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        // HitBox 활성화 시 플레이어 콜라이더와 충돌 무시
        if (playerCollider != null)
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), playerCollider, true);
    }

    private void OnDisable()
    {
        // HitBox 비활성화 시 충돌 복원
        if (playerCollider != null && GetComponent<Collider2D>() != null)
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), playerCollider, false);
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 자신 무시
        if (other.transform.IsChildOf(transform.parent)) return;

        // 적 처리
        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                SoundManager.Instance.PlaySound("ATTACK");
            }
            return;
        }

        // 파괴 가능 오브젝트 처리 (퍼즐용)
        if (other.CompareTag("Breakable"))
        {
            IBreakable breakable = other.GetComponent<IBreakable>();
            breakable?.OnBreak();
        }
    }
}
