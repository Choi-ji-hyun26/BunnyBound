using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplayEffect : IItemEffect
{
    public void ApplyEffect(PlayerCoordinator player)
    {
        SoundManager.Instance.PlaySound("ITEM");
        player.UtilityHandler.StartMiniMapDisplay();                                                                   
        Debug.Log("Map Item Used");
    }
}