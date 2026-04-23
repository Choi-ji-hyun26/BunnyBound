using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 외부 접근 차단 + 인스펙터 표시
    [SerializeField] private Transform player; // 캐릭터 Transform 
    [SerializeField] private float smooth = 5f; // 부드러운 카메라 이동 속도
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -10); // 카메라 오프셋
    private bool instantMove = false;  // 즉시 이동 플래그
    private bool isFollowing = true; // 플레이어를 따라가는지, 플레이어 죽었을 때 카메라 고정시키기 위함

    private void LateUpdate()
    {
        if (!isFollowing || player == null)
            return;
        if (instantMove)
        {
            transform.position = player.position + offset;
            instantMove = false;
        }
        else
        {
            Vector3 desiredPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, player.position + offset, smooth * Time.deltaTime);
        }
    }

    public void StopFollowing()
    {
        isFollowing = false;
    }
    public void InstantMoveTo(Vector3 position)
    {
        transform.position = position + offset;
        instantMove = false;
    }

    public void EnableInstantMoveNextFrame()
    {
        instantMove = true;
    }
}
