using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectController : MonoBehaviour
{
    public void StartEffectDuration(IDurationEffect effect)
    {
        StartCoroutine(DurationCoroutine(effect));
    }

    public IEnumerator DurationCoroutine(IDurationEffect effect)
    {
        yield return new WaitForSeconds(effect.Duration);
    }
}
