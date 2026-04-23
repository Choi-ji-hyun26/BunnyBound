using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeverTimeEffect : IItemEffect , IDurationEffect
{
    public bool HasDuration => true;
    public float Duration => 7f;

    public void ApplyEffect(PlayerCoordinator player)
    {
        SoundManager.Instance.PlaySound("ITEM");
        player.FeverHandler.HandleChest();
        Debug.Log("Fever Time ACTIVATED!");
    }
}