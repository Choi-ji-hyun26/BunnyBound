using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 힌트 목록 창 UI
/// - HintButton 클릭 시 열리고 닫힘
/// - GameProgress.GetCollectedHintIds()로 획득한 힌트 목록 동적 구성
/// - 힌트 없으면 "아직 힌트가 없습니다" 텍스트 표시
///
/// [계층 구조]
/// Hint List UI (HintListUI.cs — 항상 켜짐)
/// └── Content                ← SetActive 대상
///     ├── DimBackground      ← 전체화면 반투명, Panel보다 위
///     └── Panel
///         ├── EmptyText
///         ├── EntryContainer
///         └── CloseButton
/// </summary>
public class HintListUI : MonoBehaviour
{
    public static HintListUI Instance { get; private set; }

    [Header("버튼")]
    [SerializeField] private Button hintButton;       // 항상 존재하는 힌트 아이콘 버튼
    [SerializeField] private Button dimBackground;    // 빈 화면 터치로 닫기
    [SerializeField] private Button closeButton;      // X 버튼

    [Header("패널")]
    [SerializeField] private GameObject content;       // DimBackground + Panel 래퍼 (SetActive 대상)
    [SerializeField] private Transform entryContainer; // HintEntryItem 동적 생성 부모
    [SerializeField] private GameObject entryPrefab;   // HintEntryItem 프리팹
    [SerializeField] private TextMeshProUGUI emptyText; // 힌트 없을 때 표시

    [Header("힌트 데이터")]
    [SerializeField] private HintEffectSO[] hintDataList; // 인스펙터에서 SO 연결

    void Awake()
    {
        Instance = this;
        content.SetActive(false);
        hintButton.onClick.AddListener(Open);
        dimBackground.onClick.AddListener(Close);
        closeButton.onClick.AddListener(Close);
    }

    void Start() => Refresh();

    public void Open()
    {
        Refresh();
        content.SetActive(true);
    }

    public void Close() => content.SetActive(false);

    /// <summary>
    /// 획득한 힌트 목록으로 엔트리 동적 구성
    /// HintEffect에서 힌트 획득 시 호출
    /// </summary>
    public void Refresh()
    {
        // 기존 엔트리 제거
        foreach (Transform child in entryContainer)
            Destroy(child.gameObject);

        var collectedIds = GameProgress.GetCollectedHintIds();

        if (collectedIds.Count == 0)
        {
            emptyText.gameObject.SetActive(true);
            return;
        }

        emptyText.gameObject.SetActive(false);

        foreach (int hintId in collectedIds)
        {
            HintEffectSO data = FindHintData(hintId);
            if (data == null) continue;

            GameObject entry = Instantiate(entryPrefab, entryContainer);
            entry.GetComponent<HintEntryItem>().Setup(hintId);
        }
    }

    private HintEffectSO FindHintData(int hintId)
    {
        foreach (var data in hintDataList)
        {
            if (data.HintId == hintId) return data;
        }
        Debug.LogWarning($"[HintListUI] hintId {hintId}에 해당하는 HintEffectSO 없음");
        return null;
    }
}
