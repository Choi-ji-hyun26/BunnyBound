using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageHandler : MonoBehaviour
{
    // 피격 무적 플래그
    // true: 피격 무적 중 (PlayerHurtBox에서 피격 무시)
    // false: 피격 가능 상태
    public bool isDamageInvincible = false;

    private PlayerCoordinator coordinator;
    private PlayerTransformHandler transformHandler;
    private PlayerFeverHandler feverHandler;

    // Spike 레이어 충돌 토글용
    private int playerLayer;
    private int spikeLayer;

    private void Awake()
    {
        coordinator = GetComponent<PlayerCoordinator>();
        if (coordinator == null)
            Debug.LogError("PlayerDamageHandler requires PlayerCoordinator component");

        transformHandler = GetComponent<PlayerTransformHandler>();
        if (transformHandler == null)
            Debug.LogError("PlayerDamageHandler requires PlayerTransformHandler component");

        feverHandler = GetComponent<PlayerFeverHandler>();
        if (feverHandler == null)
            Debug.LogError("PlayerDamageHandler requires PlayerFeverHandler component");

        playerLayer = LayerMask.NameToLayer("Player");
        spikeLayer = LayerMask.NameToLayer("Spike");
    }

    public void OnDamaged(Vector2 targetPos)
    {
        // 무적 중이면 피격 무시 (피격 무적 or 피버 무적)
        if (isDamageInvincible || feverHandler.isUnBeatTime) return;

        // 현재 캐릭터 타입에 따라 HP 감소
        if (transformHandler.currentType == CharacterType.Knight)
            PlayerStats.instance.KnightHealthDown();
        else
            PlayerStats.instance.HealthDown();

        // 피격 무적 시작
        isDamageInvincible = true;
        coordinator.SpriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // 스파이크 물리 충돌 OFF → 피격 후 통과 가능
        Physics2D.IgnoreLayerCollision(playerLayer, spikeLayer, true);

        // 넉백
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        coordinator.Rigid.AddForce(new Vector2(dirc, 1) * 10, ForceMode2D.Impulse);

        // 애니메이션
        coordinator.Animator.SetTrigger("doDamaged");

        // 사운드
        SoundManager.Instance.PlaySound("DAMAGED");

        Invoke("OffDamaged", 3f); // 무적시간 3초
    }

    private void OffDamaged()
    {
        // 피격 무적 해제
        isDamageInvincible = false;

        // 스파이크 물리 충돌 복원
        if (!feverHandler.isUnBeatTime)
            Physics2D.IgnoreLayerCollision(playerLayer, spikeLayer, false);

        if (!feverHandler.isUnBeatTime)
            coordinator.SpriteRenderer.color = new Color(1, 1, 1, 1);
    }
}