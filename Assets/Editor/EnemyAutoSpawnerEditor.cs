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
        낮은 값 - slug , piranha , bee - 높은 값*/
    [Header("Spawn Rule")]
    [SerializeField] private EnemySpawnRule spawnRule;
    public AnimationCurve difficultyCurve;

    public GameObject stageObject;
    public Tilemap groundTilemap;
    public int enemyTotalCount = 10;

    // 유니티 에디터 메뉴에 새로운 창을 만드는 클래스
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

    // 에디터 창에 표시될 UI 구성
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
    // 메인 함수
    private void SpawnEnemies()
    {
        if (stageObject == null || groundTilemap == null)
        {
            Debug.LogError("Stage 또는 Ground Tilemap이 지정되지 않았습니다.");
            return;
        }

        // Undo 그룹 시작
        int group = Undo.GetCurrentGroup();
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Spawn Enemies");

        // Enemies 그룹 오브젝트 설정
        Transform enemyParent = stageObject.transform.Find("Enemies");
        if (enemyParent == null)
        {
            GameObject enemyGroup = new GameObject("Enemies");
            Undo.RegisterCreatedObjectUndo(enemyGroup, "Create Enemy Group");
            enemyGroup.transform.SetParent(stageObject.transform);
            enemyGroup.transform.localPosition = Vector3.zero;
            enemyParent = enemyGroup.transform;
        }

        // 바닥 타일 기반 위치 수집
        List<Vector3> groundPositions = GetValidGroundPositions();
        if (groundPositions.Count == 0)
        {
            Debug.LogWarning("적을 배치할 수 있는 바닥 타일이 없습니다.");
            return;
        }
        // 위치 랜덤
        Shuffle(groundPositions);
        // 적 배치 루프 
        for (int i = 0; i < Mathf.Min(enemyTotalCount, groundPositions.Count); i++)
        {
            float t = i / (float)(enemyTotalCount - 1);
            GameObject prefabToSpawn = PickEnemyByDifficulty(t);

            if (prefabToSpawn != null)
            {
                Vector3 spawnPos = groundPositions[i];

                // 적이 bee인 경우 공중 배치 (y값을 올려서)
                if (prefabToSpawn.name.ToLower().Contains("bee"))
                {
                    spawnPos.y += Random.Range(1f, 3f); // 공중 배치
                }
                // 에디터 상에서 prefab 인스턴스 생성
                GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                enemy.transform.position = spawnPos;
                enemy.transform.SetParent(enemyParent);
                // Undo : Crtl + Z, 배치 취소 
                Undo.RegisterCreatedObjectUndo(enemy, "Spawn Enemy"); 
            }
        }

        Undo.CollapseUndoOperations(group);
    }

    // 타일 맵에서 배치 가능한 위치 수집
    private List<Vector3> GetValidGroundPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        BoundsInt bounds = groundTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int current = new Vector3Int(x, y, 0);
                Vector3Int above = new Vector3Int(x, y + 1, 0);

                // 바닥 타일이 있고 그 위가 비어 있으면 적이 설 수 있는 자리로 판단
                if (groundTilemap.HasTile(current) && !groundTilemap.HasTile(above))
                {
                    // Cell 좌표를 실제 월드 좌표로 변환해서 저장
                    Vector3 worldPos = groundTilemap.CellToWorld(above); 
                    positions.Add(worldPos);
                }
            }
        }

        return positions;
    }

    // 리스트를 랜덤으로 섞는 함수
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    // 난이도 커브(t:0~1)에 따라 적 종류 선택
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