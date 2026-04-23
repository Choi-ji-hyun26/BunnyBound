using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUtilityHandler : MonoBehaviour
{
    [SerializeField] private GameObject miniMapCamera;

    public void StartMiniMapDisplay()
    {
        miniMapCamera.SetActive(true);
    }
}
