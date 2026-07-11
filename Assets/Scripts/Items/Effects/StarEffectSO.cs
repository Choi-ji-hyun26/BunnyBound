using UnityEngine;

[CreateAssetMenu(fileName = "StarEffect", menuName = "GameData/Effects/StarEffect")]
public class StarEffectSO : ItemEffectSO
{
    public override void ApplyEffect(PlayerCoordinator player)
    {
        player.Stats.AddPoint(1);
        SoundManager.Instance.PlaySound(SoundType.Item);
    }
}
