using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplayEffect : IItemEffect
{
    public void ApplyEffect(PlayerCoordinator player)
    {
        SoundManager.Instance.PlaySound(SoundType.Item);
        player.UtilityHandler.StartMiniMapDisplay();                                                                       }
}