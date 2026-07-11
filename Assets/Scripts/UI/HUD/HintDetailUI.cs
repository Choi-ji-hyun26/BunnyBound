using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 힌트 상세 팝업 UI (종이 패널)
/// - HintEffect.ApplyEffect()에서 최초 획득 시 Show(hintId, text) 호출
/// - HintEntryItem.OnClick()에서 재열람 시 Show(hintId) 호출 → SO에서 텍스트 조회
/// - X 버튼 또는 DimBackground 터치로 닫기
///
/// [계층 구조]
/// HintDetailUI (HintDetailUI.cs — 항상 켜짐)
/// └── Content                ← SetActive 대상
///     ├── DimBackground      ← 전체화면 반투명, Panel보다 위
///     └── Panel (종이 텍스처)
///         ├── HintText
///         └── CloseButton (우상단 X)
/// </summary>
public class HintDetailUI : MonoBehaviour
{
    public static HintDetailUI Instance { get; private set; }

    [SerializeField] private GameObject content;      // DimBackground + Panel 래퍼 (SetActive 대상)
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button dimBackground;

    [Header("힌트 데이터")]
    [SerializeField] private HintEffectSO[] hintDataList; // 인스펙터에서 SO 연결

    void Awake()
    {
        Instance = this;
        content.SetActive(false);
        closeButton.onClick.AddListener(Close);
        dimBackground.onClick.AddListener(Close);
    }

    /// <summary>
    /// 최초 획득 시 호출 — HintEffect에서 text 직접 전달
    /// </summary>
    public void Show(int hintId, string text)
    {
        hintText.text = text;
        content.SetActive(true);
    }

    /// <summary>
    /// 재열람 시 호출 — HintEntryItem 클릭 시 SO에서 텍스트 조회
    /// </summary>
    public void Show(int hintId)
    {
        foreach (var data in hintDataList)
        {
            if (data.HintId != hintId) continue;
            hintText.text = data.HintText;
            content.SetActive(true);
            return;
        }
        Debug.LogWarning($"[HintDetailUI] hintId {hintId}에 해당하는 HintEffectSO 없음");
    }

    public void Close() => content.SetActive(false);
}
