using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    private OpenChest nearbyInteractableChest;
    private Lever nearbyLever;

    public PlayerInputHandler input;
    private PlayerTransformHandler transformHandler;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        transformHandler = GetComponent<PlayerTransformHandler>();

        if (input == null)
            Debug.LogError("PlayerInputHandler가 Player에 없습니다.");
        if (transformHandler == null)
            Debug.LogError("PlayerTransformHandler가 Player에 없습니다.");
    }

    // ───────────────────────────────────────────
    // 상자 상호작용
    // ───────────────────────────────────────────
    public void SetInteractable(OpenChest chest)
    {
        nearbyInteractableChest = chest;
    }

    public void ClearInteractable()
    {
        nearbyInteractableChest = null;
    }

    // ───────────────────────────────────────────
    // 레버 상호작용
    // ───────────────────────────────────────────
    public void SetNearbyLever(Lever lever)
    {
        nearbyLever = lever;
    }

    public void ClearNearbyLever()
    {
        nearbyLever = null;
    }

    void Update()
    {
        HandleInteractionInput();
    }

    private void HandleInteractionInput()
    {
        if (!input.InteractPressed) return;

        // 상자 상호작용
        if (nearbyInteractableChest != null)
        {
            nearbyInteractableChest.Open();
            ClearInteractable();
            return;
        }

        // 레버 상호작용 (검사 상태에서만)
        if (nearbyLever != null)
        {
            if (transformHandler.currentType != CharacterType.Knight)
            {
                Debug.Log("[Lever] 검사 상태에서만 레버를 작동할 수 있습니다.");
                return;
            }

            // Grab 애니메이션 재생
            Animator animator = GetComponent<PlayerCoordinator>().Animator;
            if (animator != null)
                animator.SetTrigger("doGrab");

            nearbyLever.Activate();
        }
    }
}
