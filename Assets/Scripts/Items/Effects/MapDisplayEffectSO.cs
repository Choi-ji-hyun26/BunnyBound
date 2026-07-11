using UnityEngine;

[CreateAssetMenu(fileName = "MapDisplayEffect", menuName = "GameData/Effects/MapDisplayEffect")]
public class MapDisplayEffectSO : ItemEffectSO
{
    public override void ApplyEffect(PlayerCoordinator player)
    {
        SoundManager.Instance.PlaySound(SoundType.Item);
        player.UtilityHandler.StartMiniMapDisplay();
    }
}
