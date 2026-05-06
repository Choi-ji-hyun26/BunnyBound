using UnityEngine;

public class PlayerDamageHandler : MonoBehaviour
{
    public bool isDamageInvincible = false;

    // 무적 지속 시간 — PlayerHurtBox.hitCooldown과 동기화됨
    public float invincibleDuration = 3f;

    [Header("Components")]
    [SerializeField] private PlayerCoordinator coordinator;
    [SerializeField] private PlayerTransformHandler transformHandler;
    [SerializeField] private PlayerFeverHandler feverHandler;
    [SerializeField] private PlayerShieldHandler shieldHandler;

    private int playerLayer;
    private int spikeLayer;

    private void Awake()
    {
        if (coordinator == null)
            Debug.LogError("[PlayerDamageHandler] coordinator가 Inspector에 연결되지 않았습니다.");
        if (transformHandler == null)
            Debug.LogError("[PlayerDamageHandler] transformHandler가 Inspector에 연결되지 않았습니다.");
        if (feverHandler == null)
            Debug.LogError("[PlayerDamageHandler] feverHandler가 Inspector에 연결되지 않았습니다.");
        if (shieldHandler == null)
            Debug.LogError("[PlayerDamageHandler] shieldHandler가 Inspector에 연결되지 않았습니다.");

        playerLayer = LayerMask.NameToLayer("Player");
        spikeLayer = LayerMask.NameToLayer("Spike");
    }

    public void OnDamaged(Vector2 targetPos, int damage)
    {
        if (isDamageInvincible || feverHandler.isUnBeatTime || (shieldHandler != null && shieldHandler.IsShielding)) return;

        // 적별 데미지 전달
        PlayerStats.instance.TakeDamage(damage);

        isDamageInvincible = true;
        coordinator.SpriteRenderer.color = new Color(1, 1, 1, 0.4f);

        Physics2D.IgnoreLayerCollision(playerLayer, spikeLayer, true);

        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        coordinator.Rigid.AddForce(new Vector2(dirc, 1) * 10, ForceMode2D.Impulse);

        coordinator.Animator.SetTrigger("doDamaged");

        SoundManager.Instance.PlaySound("DAMAGED");

        Invoke("OffDamaged", invincibleDuration);
    }

    private void OffDamaged()
    {
        isDamageInvincible = false;

        if (!feverHandler.isUnBeatTime)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, spikeLayer, false);
            coordinator.SpriteRenderer.color = Color.white;
        }
    }
}
