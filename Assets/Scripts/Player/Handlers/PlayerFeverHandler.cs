using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeverHandler : MonoBehaviour
{
    private PlayerCoordinator coordinator;
    public bool isUnBeatTime = false; // public : Player Attack/Damaged/Chest Handler 호출,무적 타임

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if (coordinator == null)
        {
            Debug.LogError("PlayerAttackHandler requires PlayerCoordinator component!");
        }
    }

    public void HandleChest() // public : ChestItems 호출
    {
        gameObject.layer = LayerMask.NameToLayer("InvinciblePlayer");

        isUnBeatTime = true;
        StartCoroutine(UnBeatTime());
        GetComponent<FeverUIController>()?.ActivateFeverMode();
        //SoundManager.Instance.PlaySound("ITEM");
    }

    private IEnumerator UnBeatTime()
    {
        int countTime = 0;

        while (countTime < 35) // 35 * 0.2 = 7초
        {
            if (countTime % 2 == 0)
                coordinator.SpriteRenderer.color = new Color32(255, 255, 255, 90); //반투명
            else
                coordinator.SpriteRenderer.color = new Color32(255, 255, 255, 180); // 좀더 진하게

            yield return new WaitForSeconds(0.2f); // 깜빡임, 35 * 0.2 = 7초

            countTime++;
        }

        gameObject.layer = LayerMask.NameToLayer("Player");
        coordinator.SpriteRenderer.color = new Color32(255, 255, 255, 255);
        isUnBeatTime = false;

        yield return null;
    }
}
