using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 힌트 목록 창 안의 힌트 하나짜리 버튼
/// - HintListUI.Refresh()에서 동적으로 생성
/// - 클릭 시 HintDetailUI.Show(hintId) 호출
/// </summary>
public class HintEntryItem : MonoBehaviour
{
    [SerializeField] private Button button;

    private int hintId;

    public void Setup(int hintId)
    {
        this.hintId = hintId;
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        HintDetailUI.Instance?.Show(hintId);
        HintListUI.Instance?.Close();
    }
}
