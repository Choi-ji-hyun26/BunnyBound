using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform; // 카메라의 Transform 컴포넌트

    private Vector3 cameraStartPos; // 게임 시작시 카메라 시작 위치
    private float distance; // cameraStartPos부터 현재 카메라까지의 x 이동거리

    private Material[] materials; // 배경 스크롤을 위한 Material 배열 변수
    private float[] layerMoveSpeed; // z 값이 다른 배경 레이어 별 이동속도

    [SerializeField][Range(0.01f, 1.0f)] float ParallaxSpeed = 0.018f; // layerMoveSpeed에 곱해서 사용하는 배경 스크롤 이동 속도

    private void Awake()
    {
        // 게임을 시작할 때 카메라의 위치 저장(이동 거리 계산용)
        cameraStartPos = cameraTransform.position;

        // 배경의 개수를 구하고 배경 정보를 저장할 GameObject 배열 선언
        int backgroundCount = transform.childCount;
        GameObject[] backgrounds = new GameObject[backgroundCount];

        // 각 배경의 material과 이동 속도를 저장할 배열 선언
        materials = new Material[backgroundCount];
        layerMoveSpeed = new float[backgroundCount];

        // GetChild() 메소드를 호출해 자식으로 있는 배경 정보 불러옴
        for (int i = 0; i <backgroundCount; ++i)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            materials[i] = backgrounds[i].GetComponent<Renderer>().material;
        }
        //레이어(카메라의 z 거리 기준) 별로 이동 속도 설정
        CalculateMoveSpeedLayer(backgrounds, backgroundCount);
    }

    private void CalculateMoveSpeedLayer(GameObject[] backgrouds, int count)
    {
        float farthestBackDistance = 0;
        for(int i = 0; i < count; ++i)
        {
            if((backgrouds[i].transform.position.z - cameraTransform.position.z) > farthestBackDistance)
            {
                farthestBackDistance = backgrouds[i].transform.position.z - cameraTransform.position.z;
            }
        }

        for (int i =0 ; i <count; ++ i)
        {
            layerMoveSpeed[i] = 1 - (backgrouds[i].transform.position.z - cameraTransform.position.z) / farthestBackDistance;
            Debug.Log($"{layerMoveSpeed[i]}, 실제 이동속도 = {layerMoveSpeed[i] * ParallaxSpeed}");
        }
    }

    private void LateUpdate() { //각 레이어가 다른 속도로 움직이도록 만들어서 입체감을 줌
        distance = cameraTransform.position.x - cameraStartPos.x;
        transform.position = new Vector3(cameraTransform.position.x, transform.position.y, 0);

        for(int i = 0; i < materials.Length; ++i)
        {
            float speed = layerMoveSpeed[i] * ParallaxSpeed;
            materials[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}