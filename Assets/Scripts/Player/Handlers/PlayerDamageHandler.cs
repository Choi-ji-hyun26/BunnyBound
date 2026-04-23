using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageHandler : MonoBehaviour
{
    private PlayerCoordinator coordinator;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if(coordinator == null)
        {
            Debug.LogError("PlayerDamageHandler requires PlayerCoordinator component");
        }
    }
    public void OnDamaged(Vector2 targetPos) // public : PlayerAttackHandler 호출
    {
        // Health Down
        PlayerStats.instance.HealthDown();

        //Change Layer (Immortal Active)
        gameObject.layer = LayerMask.NameToLayer("PlayerDamaged"); 
        coordinator.SpriteRenderer.color = new Color(1, 1, 1, 0.4f); //0.4 : 투명도

        //Reaction Force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1; //왼쪽에서 맞으면 왼쪽으로, 오른쪽에서 맞으면 오른쪽으로 튕기기
        coordinator.Rigid.AddForce(new Vector2(dirc, 1) * 10, ForceMode2D.Impulse);

        // Animation
        coordinator.Animator.SetTrigger("doDamaged");

        //Sound
        SoundManager.Instance.PlaySound("DAMAGED");

        Invoke("OffDamaged", 3); // 무적시간 3초
    }

    private void OffDamaged()
    {
        if (GetComponentInParent<PlayerFeverHandler>().isUnBeatTime) // 데미지 입은 상태에 피버타임 겹치는 경우 layer 변동이 없도록
            return;
        gameObject.layer = LayerMask.NameToLayer("Player");
        coordinator.SpriteRenderer.color = new Color(1, 1, 1, 1);
    }
}