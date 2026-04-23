using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    private PlayerCoordinator coordinator;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if (coordinator == null)
        {
            Debug.LogError("PlayerDeathHandler requires PlayerCoordinator component");
        }
    }
    public void OnDie() // public : PlayerDeathHandler 호출
    {
        //Sprite Alpha
        coordinator.SpriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //Sprite Flip Y
        coordinator.SpriteRenderer.flipY = true;
        //Collider Disable
        coordinator.BoxCollider.enabled = false;
        //Die Effect Jump
        coordinator.Rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // 카메라 따라가기 멈추기
        FindObjectOfType<CameraController>().StopFollowing();
        //Sound
        SoundManager.Instance.PlaySound("DIE");
    }
}
