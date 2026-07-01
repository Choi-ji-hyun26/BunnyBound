using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 커스텀 플로팅 조이스틱
/// 활성 영역(이 오브젝트의 RectTransform, 보통 화면 왼쪽 절반) 안을 터치하면
/// 그 지점에 배경 링과 핸들이 나타나고 드래그하면 핸들이 배경 안에서 이동
/// 손을 떼면 사라지며 입력을 0으로 되돌림
///
/// 계산된 방향 벡터는 PlayerInputHandler.SetJoystickInput으로 전달되어
/// 기존 Move 입력을 오버라이드 (터치 중일 때만 우선 적용)
///
/// [배치]
/// - 이 스크립트는 활성 영역을 정의하는 RectTransform에 붙임
///   Image(Raycast Target 켬, alpha 0의 투명 이미지)를 함께 두어 터치를 받음
///   왼쪽 절반만 쓰려면 anchor를 min(0,0) max(0.5,1)로 stretch 
/// - background/handle은 이 오브젝트의 자식으로 두고 인스펙터에서 연결
///
/// [좌표계 주의]
/// 핸들/배경 위치는 이 RectTransform 기준 로컬 좌표로 계산
/// ScreenPointToLocalPointInRectangle에 넘기는 기준을 항상 이 RectTransform으로
/// 통일해야 부모 오프셋으로 인한 어긋남이 생기지 않음
/// </summary>
public class FloatingJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("참조")]
    [SerializeField] private RectTransform background; // 고정 링 (터치 지점에 표시)
    [SerializeField] private RectTransform handle;     // 움직이는 점
    [SerializeField] private PlayerInputHandler inputHandler;

    [Header("설정")]
    [Tooltip("핸들이 배경 중심에서 벗어날 수 있는 최대 반경(px)")]
    [SerializeField] private float movementRange = 60f;

    [Tooltip("이 값(0~1) 이하의 입력은 0으로 처리해 미세 떨림을 무시한다")]
    [Range(0f, 1f)]
    [SerializeField] private float deadZone = 0.1f;

    private RectTransform areaRect;   // 활성 영역(이 오브젝트)의 RectTransform
    private CanvasGroup visualGroup;  // background+handle을 함께 표시/숨김

    private void Awake()
    {
        areaRect = GetComponent<RectTransform>();

        // background/handle을 한 번에 켜고 끄기 위한 CanvasGroup.
        // 이 오브젝트(JoystickArea) 자신에 붙은 CanvasGroup을 사용한다.
        // 구조를 JoystickArea 바로 아래에 background/handle을 두어
        // 좌표계 불일치(부모 오프셋)를 없앱니다.
        visualGroup = GetComponent<CanvasGroup>();

        SetVisible(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 터치 지점을 활성 영역 로컬 좌표로 변환
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                areaRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        // 배경과 핸들을 터치 지점으로 이동
        if (background != null) background.anchoredPosition = localPoint;
        if (handle != null) handle.anchoredPosition = localPoint;

        SetVisible(true);

        // 누른 순간에도 방향 계산(드래그 없이 탭만 해도 반응하도록)
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                areaRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        // 배경 중심(터치 시작점) 기준 현재 손가락까지의 벡터
        Vector2 origin = background != null ? background.anchoredPosition : Vector2.zero;
        Vector2 delta = localPoint - origin;

        // movementRange로 정규화된 입력 벡터 (크기 0~1)
        Vector2 input = Vector2.ClampMagnitude(delta, movementRange) / movementRange;

        // 데드존 처리
        if (input.magnitude < deadZone)
            input = Vector2.zero;

        // 핸들은 movementRange 내로 제한된 위치에 배치
        if (handle != null)
            handle.anchoredPosition = origin + Vector2.ClampMagnitude(delta, movementRange);

        if (inputHandler != null)
            inputHandler.SetJoystickInput(input);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetVisible(false);

        if (inputHandler != null)
            inputHandler.ClearJoystickInput();
    }

    private void SetVisible(bool visible)
    {
        if (visualGroup != null)
        {
            // alpha만 토글. blocksRaycasts는 건드리지 않는다.
            // 활성 영역(areaRect)의 Image가 터치를 받아야 하므로,
            // 비주얼 그룹의 raycast 여부는 영향이 없어 alpha만으로 충분하다.
            visualGroup.alpha = visible ? 1f : 0f;
        }
    }

    private void OnDisable()
    {
        // 씬 전환이나 오브젝트 비활성화로 OnPointerUp이 호출되지 못하는 경우에 대비.
        // 이 가드가 없으면 hasJoystickInput이 true로 남아있어
        // 복귀 후 캐릭터가 마지막 입력 방향으로 계속 움직이는 유령 입력 버그가 발생한다.
        if (inputHandler != null)
            inputHandler.ClearJoystickInput();

        SetVisible(false);
    }
}
