using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemEffect
{
    // 아이템의 실제 효과를 적용
    void ApplyEffect(PlayerCoordinator player);
}
