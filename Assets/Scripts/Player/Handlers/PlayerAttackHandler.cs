using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHandler : MonoBehaviour
{
    private PlayerCoordinator coordinator;
    private PlayerTransformHandler transformHandler;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if(coordinator == null)
            Debug.LogError("PlayerAttackHandler requires PlayerCoordinator component");

        transformHandler = GetComponent<PlayerTransformHandler>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 검사 상태에서는 토끼 점프 공격 비활성화
        // 검사는 SwordHitBox에서 데미지 처리
        if (transformHandler != null && transformHandler.currentType == CharacterType.Knight)
            return;

        if (collision.gameObject.tag == "Enemy")
        {
            if (coordinator.FeverHandler.isUnBeatTime)
            {
                OnAttack(collision.transform);
            }
            else if (coordinator.Rigid.velocity.y < -0.01f && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            else
            {
                GetComponentInParent<PlayerDamageHandler>().OnDamaged(collision.transform.position);
            }
        }
    }

    private void OnAttack(Transform enemy)
    {
        if (enemy == null)
            return;

        //Reaction Force
        if (!coordinator.FeverHandler.isUnBeatTime) // 무적이 아닌 기본 상태에서 공격은 점프! 무적은 아무 움직임 x
            coordinator.Rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        //Enemy Die
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase == null)
            return;
        //enemyBase.OnDamaged();
        enemyBase.TakeDamage(5); // 토끼 점프 공격 데미지 (검사 Q:10 대비 절반)

        //Sound
        SoundManager.Instance.PlaySound("ATTACK");
    }
}
