using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 퍼즐 게이트 UI (문 이미지 + 심볼 버튼 2행 3열)
/// - PuzzleGateTrigger.OpenPuzzleUI()에서 Open(SequenceGate) 호출
/// - 심볼 버튼 클릭 시 SequenceGate.OnSymbolSelected() 호출
/// - 진행 슬롯: 입력할 때마다 filledSlotColor로 변경
/// - 오답 피드백: 슬롯 빨갛게 + 패널 Shake 후 리셋
///
/// [인스펙터 설정]
/// - content: DimBackground + Panel 래퍼 (SetActive 대상)
/// - symbolButtons: 6개 버튼 (Star, Moon, Sun, Carrot, BittenCarrot, Rabbit 순서)
/// - progressSlots: 6개 슬롯 Image
/// - panelRect: Shake 대상 RectTransform
/// </summary>
public class PuzzleGateUI : MonoBehaviour
{
    public static PuzzleGateUI Instance { get; private set; }

    [Header("패널")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button dimBackground;
    [SerializeField] private RectTransform panelRect;

    [Header("심볼 버튼 (Star / Moon / Sun / Carrot / BittenCarrot / Rabbit 순)")]
    [SerializeField] private Button[] symbolButtons;

    [Header("진행 슬롯")]
    [SerializeField] private Image[] progressSlots;
    [SerializeField] private Color defaultSlotColor = Color.white;
    [SerializeField] private Color filledSlotColor = Color.yellow;
    [SerializeField] private Color wrongSlotColor = Color.red;

    [Header("오답 피드백")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 10f;

    private SequenceGate currentGate;

    void Awake()
    {
        Instance = this;
        content.SetActive(false);
        closeButton.onClick.AddListener(Close);
        dimBackground.onClick.AddListener(Close);
    }

    /// <summary>
    /// PuzzleGateTrigger에서 호출 — SequenceGate 주입 후 열기
    /// </summary>
    public void Open(SequenceGate gate)
    {
        currentGate = gate;
        SetupButtons(gate);
        ResetProgress();
        content.SetActive(true);
    }

    public void Close()
    {
        currentGate?.ResetInput();
        content.SetActive(false);
    }

    /// <summary>
    /// 심볼 버튼에 SequenceGate.OnSymbolSelected() 리스너 등록
    /// PuzzleSymbol enum 순서와 symbolButtons 배열 순서를 맞춰야 합니다
    /// </summary>
    private void SetupButtons(SequenceGate gate)
    {
        PuzzleSymbol[] symbols = (PuzzleSymbol[])System.Enum.GetValues(typeof(PuzzleSymbol));
        for (int i = 0; i < symbolButtons.Length; i++)
        {
            symbolButtons[i].onClick.RemoveAllListeners();
            PuzzleSymbol symbol = symbols[i];
            symbolButtons[i].onClick.AddListener(() => gate.OnSymbolSelected(symbol));
        }
    }

    /// <summary>
    /// 심볼 입력 시 SequenceGate에서 호출
    /// </summary>
    public void UpdateProgress(int count)
    {
        if (count <= progressSlots.Length)
            progressSlots[count - 1].color = filledSlotColor;
    }

    public void ResetProgress()
    {
        foreach (var slot in progressSlots)
            slot.color = defaultSlotColor;
    }

    /// <summary>
    /// 오답 시 SequenceGate에서 호출
    /// 슬롯 빨갛게 + 패널 Shake 후 리셋
    /// </summary>
    public void OnWrong()
    {
        foreach (var slot in progressSlots)
            slot.color = wrongSlotColor;

        panelRect.DOShakePosition(shakeDuration, shakeStrength)
            .OnComplete(ResetProgress);
    }
}
