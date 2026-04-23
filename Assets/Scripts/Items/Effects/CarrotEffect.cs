using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrotEffect : IItemEffect
{
    public void ApplyEffect(PlayerCoordinator player)
    {
        player.Stats.HealthUp();
        SoundManager.Instance.PlaySound("ITEM");
    }
}
