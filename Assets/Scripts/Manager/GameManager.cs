using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private PlayerStateMachine player;

    [SerializeField] private GameObject[] Stages;
    private int currentStageIndex;

    [SerializeField] private TextMeshProUGUI UIStage;

    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject UIRestartBtn;
    [SerializeField] private GameObject miniMapCamera;
    [SerializeField] private GameObject SettingMenu;

    private int collectedStarCount = 0;
    public int totalStarCount = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StageIdentifier info = FindObjectOfType<StageIdentifier>();
        totalStarCount = info.totalStarCount;

        // 스테이지 진입 시 player 스냅샷 저장
        // 클리어 없이 이탈 시 RollbackPlayer()로 복원
        GameProgress.TakeSnapshot();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
            InitializeStageFromProgress();
    }

    public void InitializeStageFromProgress()
    {
        currentStageIndex = GameProgress.CurrentStageId - 1;
        ActivateStage(currentStageIndex);
        UIStage.text = "STAGE " + GameProgress.CurrentStageId;
    }

    private void ActivateStage(int index)
    {
        for (int i = 0; i < Stages.Length; i++)
            Stages[i].SetActive(i == index);
    }

    private int CalculateStarRank(int collected, int total)
    {
        if (total <= 0) return 1;
        if (collected >= total) return 3;
        if (collected >= total / 2) return 2;
        return 1;
    }

    public void OnStageCleared()
    {
        StageIdentifier stageInfo = FindObjectOfType<StageIdentifier>();
        int stageId = stageInfo.StageId;
        int totalStarCount = stageInfo.totalStarCount;
        int collectedStarCount = PlayerStats.instance.stagePoint;

        int starRank = CalculateStarRank(collectedStarCount, totalStarCount);

        GameProgress.UpdateStageResult(stageId, collectedStarCount, starRank);
        GameProgress.CommitSnapshot(); // 스냅샷 폐기 — 클리어로 확정
        GameProgress.SaveImmediate();
        Debug.Log($"Cleared Stage: {stageId}");

        PlayerStats.instance.ResetForNextStage();

        if (currentStageIndex < Stages.Length - 1)
            StartCoroutine(TransitionToNextStage());
        else
        {
            Time.timeScale = 0;
            LoadScenes();
        }
    }

    public IEnumerator TransitionToNextStage()
    {
        miniMapCamera.SetActive(false);
        player.CanMove = false;

        yield return StartCoroutine(FadeController.Instance.FadeOut());

        currentStageIndex++;
        ActivateStage(currentStageIndex);

        // 다음 스테이지 진입 시 새 스냅샷 저장
        GameProgress.TakeSnapshot();

        PlayerReposition();

        var cam = Camera.main.GetComponent<CameraController>();
        if (cam != null)
            cam.EnableInstantMoveNextFrame();

        UIStage.text = "STAGE " + (currentStageIndex + 1);

        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(FadeController.Instance.FadeIn());

        player.CanMove = true;
    }

    private void LoadScenes() => SceneManager.LoadScene("StageSelect");

    // 낙사 안전망 — 맵 하단 트리거 감지 시 리포지션 + 하트 1개 데미지
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (PlayerStats.instance.CurrentHP > PlayerStats.HP_PER_HEART)
            PlayerReposition();

        PlayerStats.instance.TakeDamage(PlayerStats.HP_PER_HEART);
    }

    private void PlayerReposition()
    {
        player.transform.position = new Vector3(-6, -1, -10);
        player.VelocityZero();
    }

    public void ViewBtn()
    {
        darkOverlay.SetActive(true);
        UIRestartBtn.SetActive(true);
    }

    public void Restart()
    {
        // 클리어 없이 재시작 — player 데이터 롤백
        GameProgress.RollbackPlayer();

        Destroy(GameManager.Instance.gameObject);
        Destroy(PlayerStats.instance.gameObject);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void BackToStageSelect()
    {
        // 클리어 없이 이탈 — player 데이터 롤백
        GameProgress.RollbackPlayer();

        Time.timeScale = 1f;
        SceneManager.LoadScene("StageSelect");
    }

    public void Exit() => Application.Quit();
}
