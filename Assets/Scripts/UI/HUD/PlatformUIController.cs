using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformUIController : MonoBehaviour
{
    private void Awake()
    {
        // 씬 로드 직후 바로 판단
        gameObject.SetActive(Application.isMobilePlatform);
    }
}
