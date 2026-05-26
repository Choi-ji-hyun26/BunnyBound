using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarEffect : IItemEffect
{
    public void ApplyEffect(PlayerCoordinator player)
    {
        player.Stats.AddPoint(1);
        SoundManager.Instance.PlaySound(SoundType.Item);
    }
}
