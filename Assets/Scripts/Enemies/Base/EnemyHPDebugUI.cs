using UnityEngine;
using TMPro;

/// <summary>
/// 적 HP 디버그용 UI
/// - 적 머리 위에 현재 HP / 최대 HP 표시
/// - 테스트 용도, 추후 실제 HP바로 교체 예정
/// </summary>
public class EnemyHPDebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshPro hpText;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.2f, 0f);

    private EnemyBase enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyBase>();

        if (hpText == null)
            hpText = GetComponent<TextMeshPro>();

        if (enemy == null)
            Debug.LogWarning("[EnemyHPDebugUI] EnemyBase를 찾을 수 없습니다.");

        // 로컬 포지션 고정 (자식 오브젝트라 부모 기준 offset)
        transform.localPosition = localOffset;
        // 로컬 스케일 정상화
        transform.localScale = Vector3.one;
    }

    private void LateUpdate()
    {
        if (enemy == null || hpText == null) return;

        // HP 텍스트 갱신
        hpText.text = $"{enemy.CurrentHp} / {enemy.MaxHp}";

        // 카메라를 향하도록 회전 (빌보드 효과)
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;
    }
}
