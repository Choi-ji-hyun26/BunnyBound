using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeverHandler : MonoBehaviour
{
    private PlayerCoordinator coordinator;
    public bool isUnBeatTime = false;

    [Header("Fever HitBox")]
    [SerializeField] private FeverHitBox feverHitBox; // Inspector에서 직접 연결

    // 스파이크(Spike 레이어) 통과를 위한 레이어 캐싱
    private int playerLayer;
    private int spikeLayer;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if (coordinator == null)
            Debug.LogError("PlayerFeverHandler requires PlayerCoordinator component!");

        playerLayer = LayerMask.NameToLayer("Player");
        spikeLayer = LayerMask.NameToLayer("Spike");
    }

    public void HandleChest()
    {
        isUnBeatTime = true;

        // 피버 HitBox 활성화 (몸통 자체가 공격)
        if (feverHitBox != null)
            feverHitBox.Activate();

        // 스파이크(Spike 레이어) 물리 충돌 OFF → 통과 가능
        Physics2D.IgnoreLayerCollision(playerLayer, spikeLayer, true);

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
                coordinator.SpriteRenderer.color = new Color32(255, 255, 255, 90);
            else
                coordinator.SpriteRenderer.color = new Color32(255, 255, 255, 180);

            yield return new WaitForSeconds(0.2f);
            countTime++;
        }

        // 피버 종료
        isUnBeatTime = false;

        // 피버 HitBox 비활성화
        if (feverHitBox != null)
            feverHitBox.Deactivate();

        // 스파이크 물리 충돌 복원
        Physics2D.IgnoreLayerCollision(playerLayer, spikeLayer, false);

        coordinator.SpriteRenderer.color = new Color32(255, 255, 255, 255);

        yield return null;
    }
}
