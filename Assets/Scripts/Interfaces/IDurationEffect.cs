using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDurationEffect
{
    // 이펙트가 지속 시간을 가지는지 여부(피버 클로버 : true, 맵 : false)
    bool HasDuration { get; }
    // 이펙트 지속 시간(초)
    float Duration { get; }
}
