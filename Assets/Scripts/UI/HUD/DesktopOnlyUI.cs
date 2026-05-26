using UnityEngine;

/// <summary>
/// 데스크탑 전용 UI 오브젝트에 붙이는 컴포넌트.
/// 모바일 플랫폼에서는 자동으로 비활성화됩니다.
/// 대칭 컴포넌트: PlatformUIController (모바일 전용)
/// </summary>
public class DesktopOnlyUI : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(!Application.isMobilePlatform);
    }
}
