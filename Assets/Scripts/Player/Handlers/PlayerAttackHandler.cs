using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHandler : MonoBehaviour
{
    private PlayerCoordinator coordinator;
    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if(coordinator == null)
        {
            Debug.LogError("PlayerAttackHandler requires PlayerCoordinator component");
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            if (coordinator.FeverHandler.isUnBeatTime)
            {
                OnAttack(collision.transform);
            }
            else if (coordinator.Rigid.velocity.y < -0.01f && transform.position.y > collision.transform.position.y)
            { // 점프 공격 또는 무적상태일때
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
        enemyBase.TakeDamage(1);

        //Sound
        SoundManager.Instance.PlaySound("ATTACK");
    }
}
