using UnityEngine;

[CreateAssetMenu(fileName = "CarrotEffect", menuName = "GameData/Effects/CarrotEffect")]
public class CarrotEffectSO : ItemEffectSO
{
    public override void ApplyEffect(PlayerCoordinator player)
    {
        player.Stats.HealthUp();
        SoundManager.Instance.PlaySound(SoundType.Item);
    }
}
