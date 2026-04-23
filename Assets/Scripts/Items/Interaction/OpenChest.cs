using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenChest : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite opendChest;
    [SerializeField] private GameObject containedItemObject;
    [SerializeField] private string originalItemLayer = "ChestItem"; // 아이템의 원래 레이어 이름
    //[SerializeField] private string tempIgnoreLayer = "TempIgnoreItem";       // 임시 무시 레이어 이름
    
    private int originalLayerID;
    //private int tempIgnoreLayerID;
    private bool isOpened = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Awake()
    {
        originalLayerID = LayerMask.NameToLayer(originalItemLayer);
        //tempIgnoreLayerID = LayerMask.NameToLayer(tempIgnoreLayer);
    }

    public void Open()
    {
        if (isOpened) return;
        isOpened = true;
        spriteRenderer.sprite = opendChest;

        if (containedItemObject != null)
        {
            containedItemObject.SetActive(true);

            Rigidbody2D itemBody = containedItemObject.GetComponent<Rigidbody2D>();

            if (itemBody != null)
            {
                Vector3 targetPos = containedItemObject.transform.position + new Vector3(0, 2.5f, 0);

                StartCoroutine(RiseAndActivateItem(containedItemObject, targetPos));
            }
        }
    }
    private IEnumerator RiseAndActivateItem(GameObject itemInstance, Vector3 targetPos)
    {
        while (itemInstance.transform.position != targetPos)
        {
            itemInstance.transform.position = Vector3.MoveTowards(
                itemInstance.transform.position,
                targetPos,
                5 * Time.deltaTime // 속도 제어
            );
            yield return null; // 다음 프레임까지 대기
        }

        // layer collision 활성화된 ChestItem 레이어로 교체, 기존 레이어 : player - (비활) - TempIgnoreItem
        itemInstance.layer = originalLayerID; 
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !isOpened)
        {
            PlayerInteractionHandler handler = collision.GetComponent<PlayerInteractionHandler>();
            if (handler != null)
            {
                handler.SetInteractable(this); 
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerInteractionHandler handler = collision.GetComponent<PlayerInteractionHandler>();
            if (handler != null)
            {
                handler.ClearInteractable(); 
            }
        }
    }
}