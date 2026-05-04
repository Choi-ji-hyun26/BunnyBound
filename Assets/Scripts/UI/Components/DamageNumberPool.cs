using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데미지 숫자 오브젝트 풀 — 싱글톤
/// - GC 부하 없이 DamageNumber 재사용
/// - DamageNumber.prefab을 Inspector에서 연결
/// </summary>
public class DamageNumberPool : MonoBehaviour
{
    public static DamageNumberPool Instance { get; private set; }

    [SerializeField] private DamageNumber prefab;
    [SerializeField] private int initialPoolSize = 10;

    private readonly Queue<DamageNumber> pool = new Queue<DamageNumber>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 초기 풀 생성
        for (int i = 0; i < initialPoolSize; i++)
            pool.Enqueue(CreateNew());
    }

    /// <summary>
    /// 풀에서 꺼내서 피격 위치에 데미지 숫자 표시
    /// </summary>
    public void Get(int damage, Vector3 worldPosition)
    {
        DamageNumber number = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        number.Play(damage, worldPosition);
    }

    /// <summary>
    /// 애니메이션 완료 후 풀에 반환
    /// </summary>
    public void Return(DamageNumber number)
    {
        number.gameObject.SetActive(false);
        pool.Enqueue(number);
    }

    private DamageNumber CreateNew()
    {
        DamageNumber number = Instantiate(prefab, transform);
        number.gameObject.SetActive(false);
        return number;
    }
}
