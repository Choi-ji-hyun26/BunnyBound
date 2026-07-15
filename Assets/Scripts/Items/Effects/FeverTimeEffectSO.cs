using UnityEngine;

[CreateAssetMenu(fileName = "FeverTimeEffect", menuName = "GameData/Effects/FeverTimeEffect")]
public class FeverTimeEffectSO : ItemEffectSO, IDurationEffect
{
    [SerializeField] private float duration = 7f;

    public bool HasDuration => true;
    public float Duration => duration;

    public override void ApplyEffect(PlayerCoordinator player)
    {
        SoundManager.Instance.PlaySound(SoundType.Item);
        player.FeverHandler.HandleChest();
        Debug.Log("Fever Time ACTIVATED!");
    }
}
