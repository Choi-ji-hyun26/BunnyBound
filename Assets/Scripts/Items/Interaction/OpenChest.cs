using System.Collections;
using UnityEngine;

public class OpenChest : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite opendChest;
    [SerializeField] private GameObject containedItemObject;
    [SerializeField] private string originalItemLayer = "ChestItem";

    [Header("1회용 상자 설정")]
    [Tooltip("스테이지 번호 * 100 + 상자 번호 (예: 스테이지1 → 101, 102)\n1회용 아이템이 아닌 상자는 0으로 두세요.")]
    [SerializeField] private int chestId = 0;

    private int originalLayerID;
    private bool isOpened = false;

    private void Awake()
    {
        originalLayerID = LayerMask.NameToLayer(originalItemLayer);
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 1회용 상자이고 이미 획득한 경우 — 열린 상태로 시작, 아이템 비활성화
        if (chestId > 0 && GameProgress.IsChestCollected(chestId))
        {
            isOpened = true;
            if (spriteRenderer != null && opendChest != null)
                spriteRenderer.sprite = opendChest;
            if (containedItemObject != null)
                containedItemObject.SetActive(false);
        }
    }

    public void Open()
    {
        if (isOpened) return;

        SoundManager.Instance.PlaySound("INTERACT");
        
        isOpened = true;
        spriteRenderer.sprite = opendChest;

        if (containedItemObject != null)
        {
            // 1회용 아이템 여부 확인 후 획득 기록
            CollectibleItem collectible = containedItemObject.GetComponent<CollectibleItem>();
            if (chestId > 0 && collectible != null && collectible.itemData != null && collectible.itemData.isOneTimeItem)
                GameProgress.CollectChest(chestId);

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
                5 * Time.deltaTime
            );
            yield return null;
        }

        itemInstance.layer = originalLayerID;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !isOpened)
        {
            PlayerInteractionHandler handler = collision.GetComponent<PlayerInteractionHandler>();
            if (handler != null)
                handler.SetInteractable(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerInteractionHandler handler = collision.GetComponent<PlayerInteractionHandler>();
            if (handler != null)
                handler.ClearInteractable();
        }
    }
}
