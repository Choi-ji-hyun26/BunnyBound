using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleItem : MonoBehaviour 
{
    public ItemData itemData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCoordinator player = collision.GetComponent<PlayerCoordinator>();
            if (player == null) return;

            IItemEffect effect = itemData.GetEffectInstance();
            if (effect == null) return;

            effect.ApplyEffect(player);

            IDurationEffect durationEffect = effect as IDurationEffect; // IDurationEffect 인터페이스 체크
            if (durationEffect != null)
            {
                player.EffectController.StartEffectDuration(durationEffect);
            }

            gameObject.SetActive(false);
        }
    }

}
