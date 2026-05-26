using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUtilityHandler : MonoBehaviour
{
    [SerializeField] private GameObject miniMapCamera;

    public void StartMiniMapDisplay()
    {
        if (Application.isMobilePlatform)
        {
            var cam = Camera.main.GetComponent<CameraController>();
            if (cam != null) cam.SetMapZoom(true);
        }
        else
        {
            miniMapCamera.SetActive(true);
        }
    }
}
