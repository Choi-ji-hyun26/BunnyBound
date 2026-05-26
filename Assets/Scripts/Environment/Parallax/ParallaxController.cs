using UnityEngine;

/// <summary>
/// Transform 이동 기반 Parallax 스크롤 컨트롤러
///
/// 구조:
/// - ParallaxLayer: 무한 스크롤이 필요한 레이어 (Far, Mid)
///   - 3개 오브젝트 세트(left, center, right)로 무한 스크롤 구현
///   - 카메라 이동량 * parallaxFactor만큼 이동, 화면 밖으로 나가면 반대쪽으로 재배치
/// - StaticFollowLayer: 카메라를 그대로 추종하는 레이어 (Top, Bottom)
///   - parallaxFactor = 0으로 카메라와 동일하게 이동
///
/// Inspector 설정:
/// - layers: ParallaxLayer 배열 (Far, Mid 등)
/// - staticFollowers: 카메라 추종 오브젝트 배열 (Middle_Top, Middle_Bottom)
/// - cameraTransform: Main Camera Transform 연결
/// </summary>
public class ParallaxController : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform left;
        public Transform center;
        public Transform right;

        [Range(0f, 1f)]
        public float parallaxFactor; // 0: 카메라와 동일, 1: 완전 고정

        [HideInInspector] public float spriteWidth;
    }

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private Transform[] staticFollowers; // Middle_Top, Middle_Bottom

    private float previousCameraX;

    private void Start()
    {
        previousCameraX = cameraTransform.position.x;

        foreach (var layer in layers)
        {
            // center 오브젝트의 SpriteRenderer 너비를 기준으로 spriteWidth 계산
            var sr = layer.center.GetComponent<SpriteRenderer>();
            if (sr != null)
                layer.spriteWidth = sr.bounds.size.x;
            else
                layer.spriteWidth = 25f; // fallback
        }
    }

    private void LateUpdate()
    {
        float deltaX = cameraTransform.position.x - previousCameraX;
        previousCameraX = cameraTransform.position.x;

        foreach (var layer in layers)
        {
            float move = deltaX * (1f - layer.parallaxFactor);

            layer.left.position   += new Vector3(move, 0f, 0f);
            layer.center.position += new Vector3(move, 0f, 0f);
            layer.right.position  += new Vector3(move, 0f, 0f);

            RepositionIfNeeded(layer);
        }

        // staticFollowers는 카메라 x를 그대로 추종
        foreach (var follower in staticFollowers)
        {
            follower.position = new Vector3(
                cameraTransform.position.x,
                follower.position.y,
                follower.position.z
            );
        }
    }

    /// <summary>
    /// 카메라 기준으로 화면 밖으로 나간 오브젝트를 반대쪽으로 재배치
    /// </summary>
    private void RepositionIfNeeded(ParallaxLayer layer)
    {
        float camX = cameraTransform.position.x;
        float width = layer.spriteWidth;

        Transform[] tiles = { layer.left, layer.center, layer.right };

        foreach (var tile in tiles)
        {
            // 오른쪽으로 너무 멀어진 경우
            if (tile.position.x > camX + width * 1.5f)
                tile.position -= new Vector3(width * 3f, 0f, 0f);

            // 왼쪽으로 너무 멀어진 경우
            else if (tile.position.x < camX - width * 1.5f)
                tile.position += new Vector3(width * 3f, 0f, 0f);
        }
    }
}
