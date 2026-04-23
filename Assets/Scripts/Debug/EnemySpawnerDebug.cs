using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // Handles 사용하려면 필요
#endif

public class EnemySpawnerDebug : MonoBehaviour
{
    [Header("디버그 옵션")]
    public bool showDebug = true;
    public Color lowDifficultyColor = Color.green;
    public Color midDifficultyColor = Color.yellow;
    public Color highDifficultyColor = Color.red;

    [Header("적 데이터")]
    public Transform[] enemies;
    public AnimationCurve difficultyCurve; // 0~1 구간에서 난이도 값을 매핑

#if UNITY_EDITOR
    private void OnDrawGizmos() // 씬 뷰에 그림 그리기
    {
        if (!showDebug || enemies == null || enemies.Length == 0)
            return;

        for (int i = 0; i < enemies.Length; i++)
        {
            float t = (float)i / (enemies.Length - 1); // 적 배열 인텍스를 0~1로 정규화
            float difficulty = difficultyCurve.Evaluate(t); // 난이도 값 가져오기

            Color enemyColor = GetColorByDifficulty(difficulty);
            Gizmos.color = enemyColor;

            // 구체로 위치 표시
            Gizmos.DrawSphere(enemies[i].position, 0.3f); 

            // 난이도 라벨 표시, 씬 뷰에 텍스트(난이도 값) 표시
            Handles.Label(enemies[i].position + Vector3.up * 0.5f,
                          $"Diff: {difficulty:F2}",
                          new GUIStyle()
                          {
                              fontSize = 12,
                              normal = new GUIStyleState() { textColor = enemyColor }
                          });
        }
    }
#endif

    private Color GetColorByDifficulty(float value)
    {
        if (value < 0.33f) return lowDifficultyColor; // green
        else if (value < 0.66f) return midDifficultyColor; // yellow
        else return highDifficultyColor; // red
    }
}