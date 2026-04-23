using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 인스턴스

    [SerializeField] private PlayerStateMachine player;

    [SerializeField] private GameObject[] Stages;
    private int currentStageIndex;

    [SerializeField] private TextMeshProUGUI UIStage;

    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject UIRestartBtn;
    [SerializeField] private GameObject miniMapCamera;
    [SerializeField] private GameObject SettingMenu;
    // 클리어 수준
    private int collectedStarCount = 0;
    public int totalStarCount = 0;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject); // 이미 존재하면 중복 방지
        }
        //Debug.Log($"persistentDataPath: {Application.persistentDataPath}");
    }
    private void Start()
    {
        StageIdentifier info = FindObjectOfType<StageIdentifier>();
        totalStarCount = info.totalStarCount;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            InitializeStageFromProgress();
        }
    }

    public void InitializeStageFromProgress()
    {
        currentStageIndex = StageProgress.CurrentStageId - 1;
        ActivateStage(currentStageIndex);

        UIStage.text = "STAGE " + StageProgress.CurrentStageId;
    }

    private void ActivateStage(int index)
    {
        for(int i = 0; i < Stages.Length; i++)
        {
            Stages[i].SetActive(i == index);
        }
    }
    private int CalculateStarRank(int collected, int total)
    {
        if (total <= 0)
            return 1; // 안전 처리

        if (collected >= total)
            return 3;

        if (collected >= total / 2)
            return 2;

        return 1;
    }

    public void OnStageCleared()
    {
        // 1. 현재 스테이지 정보
        StageIdentifier stageInfo = FindObjectOfType<StageIdentifier>();
        int stageId = stageInfo.StageId;
        int totalStarCount = stageInfo.totalStarCount;
        int collectedStarCount = PlayerStats.instance.stagePoint;

        // 2. 별 등급 계산
        int starRank = CalculateStarRank(collectedStarCount, totalStarCount);

        // 3. 저장 & 해금
        //StageProgress.UnlockNextStage(stageId);
        StageProgress.UpdateStageResult(stageId, collectedStarCount, starRank); 
        StageProgress.SaveImmediate();
        Debug.Log($"Cleared Stage: {stageId}");

        // 4. Hp, Score 초기화
        PlayerStats.instance.ResetForNextStage();

        // 5. 다음 스테이지 or 엔딩
        if(currentStageIndex < Stages.Length - 1)
        {
            StartCoroutine(TransitionToNextStage());
        }
        else
        {
            //Player Control Lock
            Time.timeScale = 0;
            //Ending Scene 로드
            LoadScenes();
        }
    }

    public IEnumerator TransitionToNextStage() // public : PlayerTriggerHandler에서 호출
    {
        miniMapCamera.SetActive(false);
        player.CanMove = false; // 플레이어 이동 장금

        yield return StartCoroutine(FadeController.Instance.FadeOut()); // 이미지 점점 보임

        currentStageIndex++;
        ActivateStage(currentStageIndex);

        PlayerReposition();

        var cam = Camera.main.GetComponent<CameraController>();
        if (cam != null)
            cam.EnableInstantMoveNextFrame();

        UIStage.text = "STAGE " + (currentStageIndex + 1); //stageIndex는 0부터 시작해서 +1

        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(FadeController.Instance.FadeIn()); // 이미지 점점 사라짐

        player.CanMove = true; // 플레이어 이동 복구
    }

    private void LoadScenes()
    {
        SceneManager.LoadScene("Ending");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (PlayerStats.instance.health > 1) // 마지막 체력에서 낭떨어지일때는 원위치 하지 않기
            {
                PlayerReposition();
            }
            PlayerStats.instance.HealthDown();
        }
    }

    private void PlayerReposition()
    {
        player.transform.position = new Vector3(-6, -1, -10);

        player.VelocityZero();
    }

    // 플레이어 상태 Die 인 경우
    public void ViewBtn() // public : 유니티 엔진 UI BUTTON 연결
    {
        darkOverlay.SetActive(true);
        UIRestartBtn.SetActive(true);
    }

    public void Restart() // public : EndingController에서 호출
    {
        /*
        restart 후 별 제대로 카운트 안 된 이유
        GameManager가 DontDestroyOnLoad 상태로 유지되면서 
        재시작해도 내부 값이 초기화되지 않고 남아 있기 때문
        아래의 두개의 Destroy 코드를 추가하면서 해결!
         */
        Destroy(GameManager.Instance.gameObject); // 씬 이동 전에 호출 -> restart 후에도 Score 제대로 카운트됨 
        Destroy(PlayerStats.instance.gameObject);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    public void BackToStageSelect() // public : 유니티 엔진 UI BUTTON 연결, 스테이지 선택
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StageSelect");
    }
    public void Exit() // public : 유니티 엔진 UI BUTTON 연결 ,설정 - 나가기
    {
        Application.Quit();
    }
}
