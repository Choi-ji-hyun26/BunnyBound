using UnityEngine;
using TMPro;

/// <summary>
/// 적 HP 디버그용 UI
/// - TextMeshPro 3D 방식 (World Space)
/// - 적 머리 위에 현재 HP / 최대 HP 표시
/// - 테스트 용도, 추후 실제 HP바로 교체 예정
/// </summary>
public class EnemyHPDebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshPro hpText;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.2f, 0f);

    private EnemyBase enemy;

    private void Start()
    {
        enemy = GetComponentInParent<EnemyBase>();

        if (hpText == null)
            hpText = GetComponent<TextMeshPro>();

        if (enemy == null)
            Debug.LogWarning("[EnemyHPDebugUI] EnemyBase를 찾을 수 없습니다.");

        // Start()에서 로컬 포지션 고정
        // Awake()에서 설정하면 부모 Transform 초기화 전에 실행되어 위치가 틀어질 수 있음
        transform.localPosition = localOffset;
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
