using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 버튼 — 스테이지 선택 씬에 배치
///
/// [동작]
/// - 스테이지 9 클리어 시 버튼 활성화
/// - 클릭 시 IsAllStagesPerfect() 체크
///   → true  : 진엔딩 (Ending 씬, EndingController가 trueEndingUI 활성화)
///   → false : 노말엔딩 (Ending 씬, EndingController가 normalEndingUI 활성화)
///
/// [Inspector 설정]
/// - finalStageId : 마지막 스테이지 ID (기본값 9)
/// </summary>
public class EndingButton : MonoBehaviour
{
    [SerializeField] private int finalStageId = 9;

    private void Start()
    {
        GameProgress.Load();

        // 마지막 스테이지 클리어 여부로 버튼 활성/비활성
        bool isUnlocked = GameProgress.IsStageCleared(finalStageId);
        gameObject.SetActive(isUnlocked);
    }

    public void OnEndingButtonClicked()
    {
        SceneManager.LoadScene("Ending");
    }
}
