using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCoordinator : MonoBehaviour
{   
    // 아이템/효과 관련 핸들러
    [Header("Item & Effect Handlers")]
    [SerializeField] private PlayerFeverHandler feverHandler; // 무적 시간, 시각 연출
    [SerializeField] private PlayerEffectController effectController; // 일반 효과 시간 관리
    [SerializeField] private PlayerUtilityHandler utilityHandler; // 미니맵 등 유틸리티
    [SerializeField] private PlayerStats stats; // 플레이어 스탯 관리

    [Header("Core Component")]
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private BoxCollider2D boxCollider;
    
    // 외부에서 핸들러에 접근하기 위한 Public Property
    // IItemEffect 구현체들이 Player 객체를 통해 핸들러에 접근할 수 있도록 제공
    public PlayerFeverHandler FeverHandler => feverHandler;
    public PlayerUtilityHandler UtilityHandler => utilityHandler;
    public PlayerEffectController EffectController => effectController;
    public PlayerStats Stats => stats;

    // 기본 컴포넌트 Property 제공
    public Rigidbody2D Rigid => rigid;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;
    public BoxCollider2D BoxCollider => boxCollider;

    private void Awake()
    {
        if (feverHandler == null || effectController == null || utilityHandler == null || stats == null)
        {
            Debug.LogError("Player Handler references are not fully set up in the Inspector!");
        }
        //Inspector 연결 확인 및 미연결 시 GetComponent로 초기화
        if (rigid == null) rigid = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
    }
}