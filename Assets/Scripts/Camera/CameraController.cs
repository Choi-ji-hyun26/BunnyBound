using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smooth = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -10);
    private bool instantMove = false;
    private bool isFollowing = true;

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
