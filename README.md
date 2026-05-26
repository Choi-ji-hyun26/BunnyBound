# BunnyBound

> 2D 플랫포머 액션 퍼즐  |  Unity 2D · C#  |  1인 개발
> 

토끼 수인과 검사, 두 캐릭터를 **Tab**으로 전환하며 플레이하는 2D 플랫포머입니다.    
변신 시스템이 단순 스탯 교체가 아닌 퍼즐 설계와 직접 연계되도록 구현했습니다.

<!-- 게임플레이 GIF 삽입 자리 -->
<img width="426" height="240" alt="transform_puzzle" src="https://github.com/user-attachments/assets/44b16412-ae9f-4c2e-941d-32fed767504e" />  <img width="426" height="240" alt="knight" src="https://github.com/user-attachments/assets/0a2b03d5-3675-4353-8cea-f965ea9a62d4" />

<br>

## 개요

| 항목 | 내용 |
| --- | --- |
| 엔진 | Unity 2021.3.45f2 / C# |
| 개발 기간 | 2026.04.23 ~ 2026.05.07 |
| 개발 형태 | 1인 (기획 · 설계 · 구현 전담, 아트 에셋은 무료 리소스 사용) |
| 플랫폼 | PC (Windows) |

<br>

## 핵심 구현

### 🔄 변신 시스템 (Tab)

스탯 · 콜라이더 · 애니메이터 · 스프라이트 스케일을 동시에 교체하는 복합 전환 시스템입니다.    
**이동 · 탐색은 토끼, 전투 · 상호작용은 검사**로 역할을 명확히 분리하고    
이 역할 분리가 퍼즐 설계와 직접 연결되도록 스테이지를 구성했습니다.

|  | 🐰 토끼 | ⚔️ 검사 |
| --- | --- | --- |
| 이동속도 | 빠름 (6) | 보통 (3.5) |
| 점프 | 2단 점프 (22 / 18) | 단일 점프 (15 / 12) |
| 콜라이더 | 작고 낮음 → 좁은 통로 통과 | 크고 높음 |
| 공격 | 없음 | Q 근접 Slash / W 원거리 관통 |
- `PlayerCoordinator`가 핵심 컴포넌트(Animator, BoxCollider2D, Rigidbody2D)를 중앙 캐싱
- 각 Handler가 Coordinator를 통해 참조 → 중복 GetComponent 제거
- 변신 후 `ChangeState(Idle)` 강제 전환으로 애니메이션 꼬임 방지

→ [`PlayerTransformHandler.cs`](https://www.notion.so/Assets/Scripts/Player/Handlers/PlayerTransformHandler.cs)

<br>

### 🦇 적 AI — 3종 FSM

`EnemyBase` 상속으로 피격 · 넉백 · 스턴 · 사망 처리를 공통화하고    
각 적의 행동 패턴은 `IEnemyState` 인터페이스를 구현한 독립 State 클래스로 분리했습니다.

|  | 슬라임 | 박쥐 | 피라냐 |
| --- | --- | --- | --- |
| 상태 수 | 1 | 4 | 2 |
| 감지 | 없음 | 원형 범위 + Raycast 장애물 차단 | 상방 반원 Raycast |
| 특징 | ThinkRoutine 방향 결정 | IsForceDash 모드 | 애니메이션 이벤트 공격 타이밍 |
| 쉴드 반응 | 넉백 | 넉백 | 스턴 |

**박쥐 FSM 설계 포인트**

```
Patrol → Dash → Bounce → (재감지: Dash / 미감지: Return) → Patrol
[피격 시] KnockbackEnd → IsForceDash = true → DashState (마지막 위치 추격)
```

감지 범위 밖에서 원거리 공격을 받아도 플레이어를 추격하도록    
`IsForceDash` 모드와 `LastKnownPlayerPosition`으로 구현했습니다.

→ [`Enemies/`](https://www.notion.so/Assets/Scripts/Enemies/)

<br>

### 💾 세이브 시스템

`PlayerPrefs` 대신 JSON 직렬화 기반의 구조적 세이브 시스템을 직접 설계했습니다.

```
GameProgress  (static facade)   ← 게임 로직이 호출하는 표면, SaveManager를 호출
SaveManager   (저장 엔진)        ← Dirty Flag · Atomic Write
SaveFile<T>   (래퍼)             ← version + data 직렬화 컨테이너
GameProgressData                 ← 단일 저장 파일 데이터
  ├── List<StageData>            ← 스테이지별 별점 · 최고 점수
  └── PlayerProgressData         ← 스킬 해금 · 최대 하트 · 상자 수집 기록
```

| 설계 포인트 | 내용 |
| --- | --- |
| **Atomic Write** | `.tmp`에 먼저 쓰고 `File.Replace()`로 원자적 교체 → 저장 중 종료 시에도 손상 없음 |
| **Dirty Flag** | 변경 시 메모리에만 반영, 클리어/앱 종료 시점에만 파일 IO |
| **스냅샷 롤백** | 스테이지 진입 시 player 데이터 복사 → 클리어 없이 이탈 시 롤백 → **클리어해야만 아이템 확정** |
| **버전 마이그레이션** | `while` 체인으로 버전별 누적 변환 (v0→v1: StarRank 음수 보정), `case` 추가만으로 확장 |

→ [`SaveSystem/`](https://www.notion.so/Assets/Scripts/SaveSystem/)

<br>

### 🛠️ 에디터 툴 — 난이도 커브 기반 적 자동 배치

1인 개발에서 반복적인 수동 배치 작업을 줄이기 위해 `EditorWindow`로 제작한 인에디터 툴입니다.    
실제 사용으로 적 배치 작업 시간을 약 **30% 단축**했습니다.

- `AnimationCurve`로 배치 순서별 난이도 설계 → Inspector에서 비선형 곡선 직접 조작
- `GroundTilemap` 분석으로 유효 바닥 위치만 자동 감지 → 허공 배치 방지
- 콜라이더 타입(Box / Circle)별 하단 오프셋 직접 계산 (에디터 모드에서 `bounds` 미계산 대응)
- 전체 배치를 단일 Undo 그룹으로 묶어 `Ctrl+Z` 한 번에 전체 취소
- `EnemySpawnRule` ScriptableObject로 적 종류 · 난이도 구간을 코드 수정 없이 관리

→ [`Editor/EnemyAutoSpawnerEditor.cs`](https://www.notion.so/Assets/Editor/EnemyAutoSpawnerEditor.cs)

<br>

## 트러블슈팅
### 01 — 레이어 기반 상태 관리 → bool 플래그 전환

`Physics2D` 레이어로 무적 상태를 관리하면서 피격 무적 · 피버 무적 · 스파이크 감지 등 상태가 늘어날수록 의존성이 증가했습니다.    
`OffDamaged()`에서 레이어를 복원하면 피버 무적이 해제되고 스파이크를 별도 감지하려면 레이어 분기가 추가되는 구조였습니다.    
근본 원인은 레이어 하나가 여러 상태를 동시에 표현하려 했던 것이었습니다.    
`isDamageInvincible` / `isUnBeatTime bool` 플래그로 전환해 각 상태가 독립적으로 동작하도록 개편했습니다.    

### 02 — Bat 피격 시 위로 튀어오름 (Sprite Pivot)

피격 시 Bat의 넉백 방향이 부정확하고 Hurt 애니메이션 중 위로 튀어오르는 현상이 발생했습니다.    
`velocity` 중복 적용(`KnockbackRoutine` · `BounceRoutine` 충돌)을 먼저 의심했으나 코드 검토 후 원인이 아님을 확인했습니다.    
Hurt 애니메이션을 직접 확인한 결과 스프라이트 Pivot이 Bottom으로 설정되어 애니메이션 전환 시 오브젝트 기준점이 발 아래로 이동하는 것이 원인이었습니다.    
Pivot을 Center로 수정해 해결했습니다.

### 03 — EnemyHitBox 피격 경로 이원화로 PlayerHurtBox의 hitCooldown 우회

PlayerHurtBox의 `hitCooldown`을 늘려도 플레이어 무적 상태 중 피격이 발생하는 문제가 있었습니다.
`EnemyHitBox.OnTriggerEnter2D`가 `PlayerDamageHandler.OnDamaged()`를 직접 호출하면서
`PlayerHurtBox`의 `hitCooldown` 체크를 완전히 우회하고 있었습니다.
모든 피격을 `PlayerHurtBox.HandleHit()` 단일 진입점으로 일원화해
`hitCooldown`이 항상 적용되도록 수정했습니다.

<br>

## 폴더 구조

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── Core/           # PlayerCoordinator, PlayerStateMachine
│   │   └── Handlers/       # Transform, Attack, Damage, Shield, Input
│   ├── Enemies/
│   │   ├── Base/           # EnemyBase, EnemyStateMachine, IEnemyState
│   │   ├── Slime/
│   │   ├── Bat/
│   │   └── Piranha/
│   ├── SaveSystem/         # SaveManager, GameProgress, SaveMigration
│   └── Stage/
│       └── Core/           # GameProgress, StageData
└── Editor/
    └── EnemyAutoSpawnerEditor.cs
```

<br>

## 실행 방법

1. Unity **2021.3.45f2** 이상에서 프로젝트 열기
2. `Assets/Scenes/` 에서 Title 씬 실행

<br>

## 링크 (링크 추가 예정)
