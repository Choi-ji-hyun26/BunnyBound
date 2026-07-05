using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 심볼 순서 입력 퍼즐 판정
/// - PuzzleGateTrigger가 플레이어 감지 후 PuzzleGateUI.Open(this) 호출
/// - PuzzleGateUI의 심볼 버튼 클릭 시 OnSymbolSelected() 호출
/// - 정답: 엔딩씬 전환 + 힌트 초기화
/// - 오답: 입력 리셋 + PuzzleGateUI 오답 피드백
///
/// [인스펙터 설정]
/// - answer: 정답 심볼 순서 (Star, Moon, Rabbit, Carrot, Sun, BittenCarrot)
/// - puzzleGateUI: PuzzleGateUI 컴포넌트 참조
/// - endingSceneName: 전환할 엔딩씬 이름
/// </summary>
public class SequenceGate : MonoBehaviour
{
    [SerializeField] private PuzzleSymbol[] answer;
    [SerializeField] private PuzzleGateUI puzzleGateUI;
    [SerializeField] private string endingSceneName = "Ending";

    private List<PuzzleSymbol> inputSequence = new();

    public int AnswerLength => answer.Length;

    /// <summary>
    /// PuzzleGateUI 심볼 버튼 클릭 시 호출
    /// </summary>
    public void OnSymbolSelected(PuzzleSymbol symbol)
    {
        inputSequence.Add(symbol);
        puzzleGateUI.UpdateProgress(inputSequence.Count);

        if (inputSequence.Count < answer.Length) return;

        if (CheckAnswer()) OnCorrect();
        else OnWrong();
    }

    private bool CheckAnswer()
    {
        for (int i = 0; i < answer.Length; i++)
            if (inputSequence[i] != answer[i]) return false;
        return true;
    }

    private void OnCorrect()
    {
        GameProgress.ClearHints();
        GameProgress.SaveImmediate();
        puzzleGateUI.Close();
        SceneManager.LoadScene(endingSceneName);
    }

    private void OnWrong()
    {
        inputSequence.Clear();
        puzzleGateUI.OnWrong(); // Shake + 슬롯 리셋
    }

    /// <summary>
    /// 파업 닫힌 시 호출 — 입력 시퀀스 초기화
    /// </summary>
    public void ResetInput()
    {
        inputSequence.Clear();
    }
}
