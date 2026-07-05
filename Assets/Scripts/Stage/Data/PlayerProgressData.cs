using System.Collections.Generic;
/// <summary>
/// 플레이어 성장 데이터 — GameProgressData에 중첩
/// 스테이지 진행 데이터(StageData)와 관심사 분리
/// </summary>
[System.Serializable]
public class PlayerProgressData
{
    // 스킬 해금 상태 (index 0~1 = attack1~2)
    // attack1(Q)은 항상 해금이므로 index 0은 항상 true
    public bool[] unlockedSkills = { true, false };

    // 최대 하트 수 (HP Up 아이템으로 증가)
    public int maxHearts = 3;

    // 획득한 1회용 보물상자 ID 목록
    // chestId 규칙: 스테이지 번호 * 100 + 상자 번호 (예: 스테이지1 → 101, 102)
    public List<int> collectedChestIds = new List<int>();

    // 획득한 힌트 ID 목록 (챕터 클리어 시 초기화)
    // hintId 규칙: 챕터번호 * 100 + 힌트번호 (예: 챕터1 힌트1 → 101, 힌트2 → 102)
    public List<int> collectedHintIds = new List<int>();
}
