using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class EnemyAutoSpawnerEditor : EditorWindow
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public GameObject prefab;
        public float weight = 1f;
    }

    /* 난이도 커브로 적의 종류 선택할 때 사용
        적 배치 순서 기반으로 어떤 적을 배치할 지 정함
        낮은 값 - slime, piranha, bat - 높은 값 */
    [Header("Spawn Rule")]
    [SerializeField] private EnemySpawnRule spawnRule;
    public AnimationCurve difficultyCurve;

    public GameObject stageObject;
    public Tilemap groundTilemap;
    public int enemyTotalCount = 10;

    [MenuItem("Tools/Enemy Auto Spawner")]
    public static void ShowWindow()
    {
        GetWindow<EnemyAutoSpawnerEditor>("Enemy Auto Spawner");
    }

    private void OnEnable()
    {
        if (difficultyCurve == null)
        {
            difficultyCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Enemy Auto Spawner", EditorStyles.boldLabel);
        stageObject = (GameObject)EditorGUILayout.ObjectField(
            "Stage 영역 오브젝트",
            stageObject,
            typeof(GameObject),
            true
        );
        groundTilemap = (Tilemap)EditorGUILayout.ObjectField(
            "Ground Tilemap",
            groundTilemap,
            typeof(Tilemap),
            true
        );
        spawnRule = (EnemySpawnRule)EditorGUILayout.ObjectField(
            "Enemy Spawn Rule",
            spawnRule,
            typeof(EnemySpawnRule),
            false
        );
        enemyTotalCount = EditorGUILayout.IntField("총 적 수", enemyTotalCount);

        GUILayout.Label("난이도 커브", EditorStyles.label);
        difficultyCurve = EditorGUILayout.CurveField(difficultyCurve);

        if (GUILayout.Button("배치 실행"))
        {
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        if (stageObject == null || groundTilemap == null)
        {
            Debug.LogError("Stage 또는 Ground Tilemap이 지정되지 않았습니다.");
            return;
        }

        int group = Undo.GetCurrentGroup();
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Spawn Enemies");

        Transform enemyParent = stageObject.transform.Find("Enemies");
        if (enemyParent == null)
        {
            GameObject enemyGroup = new GameObject("Enemies");
            Undo.RegisterCreatedObjectUndo(enemyGroup, "Create Enemy Group");
            enemyGroup.transform.SetParent(stageObject.transform);
            enemyGroup.transform.localPosition = Vector3.zero;
            enemyParent = enemyGroup.transform;
        }

        List<Vector3> groundPositions = GetValidGroundPositions();
        if (groundPositions.Count == 0)
        {
            Debug.LogWarning("적을 배치할 수 있는 바닥 타일이 없습니다.");
            return;
        }

        Shuffle(groundPositions);

        for (int i = 0; i < Mathf.Min(enemyTotalCount, groundPositions.Count); i++)
        {
            float t = i / (float)(enemyTotalCount - 1);
            GameObject prefabToSpawn = PickEnemyByDifficulty(t);

            if (prefabToSpawn != null)
            {
                Vector3 spawnPos = groundPositions[i];

                // Bat은 비행형 적이므로 공중 배치
                if (prefabToSpawn.name.ToLower().Contains("bat"))
                {
                    spawnPos.y += Random.Range(1f, 3f);
                }

                GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                enemy.transform.position = spawnPos;
                enemy.transform.SetParent(enemyParent);
                Undo.RegisterCreatedObjectUndo(enemy, "Spawn Enemy");
            }
        }

        Undo.CollapseUndoOperations(group);
    }

    private List<Vector3> GetValidGroundPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        BoundsInt bounds = groundTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int current = new Vector3Int(x, y, 0);
                Vector3Int above   = new Vector3Int(x, y + 1, 0);

                if (groundTilemap.HasTile(current) && !groundTilemap.HasTile(above))
                {
                    Vector3 worldPos = groundTilemap.CellToWorld(above);
                    positions.Add(worldPos);
                }
            }
        }

        return positions;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp  = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private GameObject PickEnemyByDifficulty(float t)
    {
        if (spawnRule == null)
        {
            Debug.LogError("EnemySpawnRule이 지정되지 않았습니다.");
            return null;
        }
        float value = difficultyCurve.Evaluate(t);
        return spawnRule.Pick(value);
    }
}
