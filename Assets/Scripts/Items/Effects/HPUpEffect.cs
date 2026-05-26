using UnityEngine;

/// <summary>
/// HP Up 아이템 효과
/// - 최대 하트 +1 (슬롯 해금)
/// - 전체 HP 회복
/// - 보물상자에서 획득
/// </summary>
public class HPUpEffect : IItemEffect
{
    public void ApplyEffect(PlayerCoordinator player)
    {
        player.Stats.IncreaseMaxHearts(); // 최대 하트 +1
        player.Stats.FullHeal();          // 전체 HP 회복
        SoundManager.Instance.PlaySound(SoundType.Item);
    }
}
