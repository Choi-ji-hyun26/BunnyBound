// using UnityEngine;
// using System.Collections;
// using Unity.VisualScripting;

// public class OneWayDropController : MonoBehaviour
// {
//     [Header("Dependencies")]
//     [SerializeField] private PlayerStateMachine player; // PlayerStateMachine 컴포넌트 참조
    
//     // 현재 플레이어가 접촉 중인 One-Way Platform과 Ladder Top GameObject
//     [HideInInspector] public GameObject currentOneWayPlatform; 
//     [HideInInspector] public GameObject currentLadderTop = null; 

//     // 내부 상태 변수
//     private bool isDropping = false;
//     private int playerLayer;


//     void Start()
//     {
//         playerLayer = LayerMask.NameToLayer("Player");

//         if (player == null)
//         {
//             Debug.LogError("PlayerStateMachine 참조가 누락되었습니다. 인스펙터에서 연결하세요.");
//         }
//         if (playerLayer == -1)
//         {
//             Debug.LogError("유니티에서 'Player' 레이어를 찾을 수 없습니다. 레이어 이름을 확인하세요.");
//         }
//     }

//     void Update()
//     {
//         float verticalInput = player.Input.MoveInput.y;
//         //  코루틴이 실행 중이 아닐 때만 입력을 확인
//         bool isDownInput = verticalInput < -0.5f;

//         if (!isDropping && isDownInput)
//         {
//             // 조건 결합: 아래 키 입력 AND 사다리/플랫폼 접촉
//             // PlayerStateMachine의 IsTouchingLadder() 헬퍼를 사용
//             if (player != null && player.IsTouchingLadder())
//             {
//                 if (currentOneWayPlatform != null && currentLadderTop != null) // currentLadderTop 확인 -> 사다리 상단에서 내려오는 상황에서만 한정
//                 {
//                     Debug.Log("하강 조건 충족: 사다리 접촉 & 플랫폼 참조 O");
//                     StartCoroutine(ToggleColliderMaskAndDrop());
//                 }
//                 else
//                 {
//                     Debug.Log("하강 조건 불충분: 사다리 접촉 O, 플랫폼 참조 X");
//                 }
//             }
//         }
//     }

//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         if (collision.gameObject.CompareTag("OneWayPlatform"))
//         {
//             currentOneWayPlatform = collision.gameObject;
//             Debug.Log("OnCollision: 플랫폼 참조 성공!"); 
//         }
//     }

//     private void OnCollisionExit2D(Collision2D collision)
//     {
//         if (collision.gameObject.CompareTag("OneWayPlatform"))
//         {
//             currentOneWayPlatform = null;
//         }
//     }

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("LadderTop"))
//         {
//             currentLadderTop = other.gameObject;
//             Debug.Log("사다리 상단 진입!");
//         }
//     }

//     private void OnTriggerExit2D(Collider2D other)
//     {
//         if (other.CompareTag("LadderTop"))
//         {
//             currentLadderTop = null;
//         }
//     }
//     private IEnumerator ToggleColliderMaskAndDrop()
//     {
//         isDropping = true;
        
//         PlatformEffector2D platformEffector = currentOneWayPlatform.GetComponent<PlatformEffector2D>();
        
//         if (platformEffector == null || playerLayer == -1)
//         {
//             Debug.LogError("Platform Effector 또는 Player 레이어 설정 오류.");
//             isDropping = false;
//             yield break; 
//         }

//         // 현재 Collider Mask 저장 (복구용)
//         int originalMask = platformEffector.colliderMask;
        
//         // Player 레이어만 Mask에서 제외 (비트 연산: 해당 레이어 비트 끄기)
//         platformEffector.colliderMask &= ~(1 << playerLayer);
//         Debug.Log("Player Mask 제외 완료. 하강 시작.");

//         // 잠시 대기 (플레이어가 플랫폼을 완전히 벗어날 시간)
//         yield return new WaitForSeconds(0.25f); 
        
//         // Collider Mask를 원래 상태로 복구
//         platformEffector.colliderMask = originalMask;
//         Debug.Log("Collider Mask 복구 완료.");

//         isDropping = false;
//     }
// }