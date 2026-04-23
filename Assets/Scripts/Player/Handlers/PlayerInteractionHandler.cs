using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    private OpenChest nearbyInteractableChest;
    public PlayerInputHandler input;
    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();

        if (input == null)
        {
            Debug.LogError("PlayerInputHandler가 Player에 없습니다.");
        }
    }
    public void SetInteractable(OpenChest chest)
    {
        nearbyInteractableChest = chest;
    }
    public void ClearInteractable()
    {
        nearbyInteractableChest = null;
    }
    void Update()
    {
        HandleInteractionInput();
    }
    private void HandleInteractionInput()
    {
        if(input.InteractPressed && nearbyInteractableChest != null)
        {
            nearbyInteractableChest.Open(); // 상자 열기 명령
            ClearInteractable(); // 상자를 열었으니 대상에서 제거
        }
    }
}

