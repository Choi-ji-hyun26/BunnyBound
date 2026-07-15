# BunnyBound

> 2D 플랫포머 액션 퍼즐 | Unity 2D · C# | 1인 개발

토끼 수인과 검사, 두 캐릭터를 **Tab**으로 전환하며 플레이하는 2D 플랫포머입니다.     
이동·탐색에 특화된 **토끼**와 전투·상호작용을 담당하는 **검사**의 역할 분리를 레벨 디자인과 직접 연결했습니다.

<img width="426" height="240" alt="transform_puzzle" src="https://github.com/user-attachments/assets/44b16412-ae9f-4c2e-941d-32fed767504e" />  <img width="426" height="240" alt="knight" src="https://github.com/user-attachments/assets/0a2b03d5-3675-4353-8cea-f965ea9a62d4" />

<br>

## 개요

| 항목 | 내용 |
|------|------|
| 엔진 | Unity 2021.3.45f2 / C# |
| 개발 기간 | 2026.04.23 – 2026.07 |
| 인원 | 1인 (기획·구조·구현, 아트 에셋은 무료 리소스 사용) |
| 플랫폼 | PC / Mobile |
| 패턴 | FSM, Handler 패턴, Coordinator 패턴, EnemyBase 상속 |
| 저장 | JSON 직렬화, Atomic Write, Dirty Flag, Snapshot Rollback |
| 에디터 | Custom EditorWindow, AnimationCurve, Undo API |
| 버전 관리 | Git Flow (feature / develop / main), GitHub PR 기반 워크플로우 |

<br>

## 아키텍처

### PlayerCoordinator — Handler 분리 구조

플레이어 로직을 기능별 Handler로 분리하고 `PlayerCoordinator`가 컴포넌트를 한 번만 캐싱해 각 Handler에 제공합니다.

```
PlayerCoordinator
├── Rigidbody2D, Animator, BoxCollider2D, SpriteRenderer  ← 캐싱 전담
│
├── [CORE]
│   ├── PlayerStateMachine    (Idle / Walk / Jump / Fall)
│   ├── PlayerInputHandler
│   └── PlayerTransformHandler
│
├── [COMBAT]
│   ├── PlayerSwordAttackHandler
│   ├── PlayerShieldHandler
│   ├── PlayerDamageHandler
│   └── PlayerHurtBox
│
└── [SYSTEM]
├── PlayerFeverHandler
├── PlayerDeathHandler
├── PlayerInteractionHandler
└── PlayerTriggerHandler
```

각 Handler가 `GetComponent()`를 중복 호출하는 문제를 Coordinator가 단일 캐싱·참조 제공 구조로 해결했습니다.     
기능 추가 시 새 Handler만 작성하면 되어 기존 코드를 건드리지 않습니다.

→ [`Player`](Assets/Scripts/Player/)

<br>

## 핵심 시스템

### 1. 변신 시스템

토끼 ↔ 검사 전환 시 스탯·애니메이터·콜라이더·스프라이트 스케일·UI를 순서대로 교체합니다.

|  | 🐰 토끼 | ⚔️ 검사 |
|---|---|---|
| 이동속도 | 빠름 (6) | 보통 (3.5) |
| 점프 | 2단 점프 (22 / 18) | 2단 점프 (15 / 12) |
| 콜라이더 | 작고 낮음 → 좁은 통로 통과 | 크고 높음 |
| 공격 | 없음 | Q 근접 Slash / W 원거리 관통 |

**설계 결정 — RuntimeAnimatorController 런타임 교체를 선택한 이유:**     
Animator를 자식 오브젝트로 분리하는 방식은 Handler들의 `coordinator.Animator` 단일 참조 구조를 전면 수정해야 합니다.     
RuntimeAnimatorController 교체는 구조 변경 없이 동일한 결과를 달성하므로 현 규모에서 비용 대비 효과가 높다고 판단했습니다.

→ [`PlayerTransformHandler.cs`](Assets/Scripts/Player/Handlers/PlayerTransformHandler.cs)

<br>

### 2. 무기 강화 시스템
<img width="794" height="389" alt="image" src="https://github.com/user-attachments/assets/bfaac840-edfe-40fa-8160-20e04daec0cd" />

별(재화)을 소모해 검사 공격력을 단계별로 강화하는 시스템입니다. 세이브 스키마를 v3→v4로 확장해 강화 단계를 영구 저장합니다.

```
WeaponUpgradeConfig (SO)         — 강화 단계별 데미지/비용 정의
        │
WeaponUpgradeManager              — 강화 요청 처리, 세이브 반영
        │
   ┌────┴────────────────────────────┐
WeaponUpgradePanel              PlayerSwordAttackHandler
(UI, Time.timeScale 일시정지)      (티어별 데미지 실시간 반영)
        │
UpgradeFlipbookEffect / SpendableStarsDisplay  — VFX · 별 잔액 UI 피드백
```

강화는 `WeaponUpgradeManager`가 요청을 받아 `PlayerSwordAttackHandler`의 티어별 데미지에 실시간 반영하고 동시에 세이브에도 기록해 재접속 시 강화 상태가 유지되도록 했습니다.     

→ [`Player/Core/WeaponUpgradeManager.cs`](Assets/Scripts/Player/Core/WeaponUpgradeManager.cs) · [`UI/Panels/WeaponUpgradePanel.cs`](Assets/Scripts/UI/Panels/WeaponUpgradePanel.cs)

<br>

### 3. 적 AI — EnemyBase 상속 구조

피격·넉백·스턴·사망의 공통 흐름을 `EnemyBase`에서 처리하고 각 적은 고유 상태만 정의합니다.

| | 슬라임 | 박쥐 | 피라냐 | FlyingDemon |
|---|---|---|---|---|
| 이동 | 지상 패트롤 | 공중 | 고정 | 공중 순찰 |
| 상태 수 | 1 | 4 | 3 | 4 |
| 감지 | — | Raycast 시야 | 근접 상방 반원 Raycast | Raycast 시야 |
| 실드 반응 | 넉백 | 넉백 | 스턴 | 넉백 |
| 역할 | 기본 위협 | 리듬 변주 | 지형 압박 | 원거리 압박 (중간 보스) |


### Bat — IsForceDash: 버그 수정에서 설계로

```
[Patrol] ──감지──▶ [Dash] ──충돌──▶ [Bounce]
   ▲                  │                 │
   │                미감지           재감지 → [Dash]
   │                  ▼            미감지 ↓
   └──────────── [Return] ◀─────────────┘
```

`DashState`가 피격 후 진입해도 감지 범위 밖이면 첫 프레임에 즉시 `ReturnState`로 전환되는 문제가 있었습니다.     
`IsForceDash` 플래그와 `LastKnownPlayerPosition`을 추가해 ForceDash 중에는 감지 체크를 건너뛰고 마지막 위치로 돌진하도록 수정했습니다.     
그 결과 원거리 공격 시 Bat이 플레이어를 추격하는 자연스러운 적 반응으로 발전했습니다.

### FlyingDemon — IShieldBlockable 인터페이스 기반 투사체 차단

공중에서 Fireball을 발사하는 중간 보스입니다.      
`Patrol → Chase → Attack → Chase` 순환 FSM이며 `attackCooldown` 동안 Flying 상태를 유지해 플레이어에게 공격 틈을 확보해줍니다.

`FireBall`은 `EnemyBase`를 상속하지 않는 독립 투사체입니다.    
검사 쉴드에 막혀야 하는데 기존 실드 판정(`ShieldHitBox` → 각 적 타입 직접 참조)에 새 투사체 타입을 추가할 때마다 `ShieldHitBox`를 수정해야 하는 구조였습니다.    `IShieldBlockable` 인터페이스를 두고 `ShieldHitBox`가 `GetComponent<IShieldBlockable>()`로 체크하도록 바꿔 `EnemyBase` 계층과 무관한 오브젝트도 쉴드 차단에 참여할 수 있게 했습니다.

→ [`Enemies/Bat/`](Assets/Scripts/Enemies/Bat/) · [`Enemies/FlyingDemon/`](Assets/Scripts/Enemies/FlyingDemon/) · [`Interfaces/IShieldBlockable.cs`](Assets/Scripts/Interfaces/IShieldBlockable.cs)

<br>

### 4. 세이브 시스템

```
Layer 01 · Facade  → GameProgress        (게임 로직이 호출하는 표면)
Layer 02 · Engine  → SaveManager         (파일 IO · Dirty 플래그 · 원자적 쓰기)
Layer 03 · Wrapper → SaveFile<T>         (제네릭 직렬화 컨테이너)
Layer 04 · Data    → GameProgressData    (단일 파일 + 내부 구조체)
```

| 포인트 | 내용 |
|--------|------|
| Atomic Write | `.tmp` 작성 후 `File.Replace()` 교체 — 저장 중 크래시에도 파일 무결성 보장 |
| Dirty Flag | 변경 시 메모리만 반영, 클리어·종료 시점에만 파일 IO |
| Snapshot Rollback | 스테이지 진입 시 스냅샷 저장 → 클리어하지 않고 이탈 시 복구 |
| Stage Cache | `Dictionary<int, StageData>` 인덱싱 — List 선형 탐색 대신 O(1) 조회 |
| Version Migration | `while` 체인으로 버전별 누적 변환 — 새 버전은 `case` 추가만으로 확장 |
| Edit Mode 테스트 | `SaveMigrationTests` 7개 케이스 — `InternalsVisibleTo`로 internal 멤버에 접근해 버전별 마이그레이션 회귀 방지 |

→ [`SaveSystem/`](Assets/Scripts/SaveSystem/)

<br>

### 5. 에디터 툴 — 난이도 커브 기반 적 자동 배치

Tilemap을 스캔해 유효 바닥 위치를 수집하고 AnimationCurve 기반 난이도 곡선으로 적을 자동 배치합니다.       

<img width="2000" height="628" alt="image" src="https://github.com/user-attachments/assets/f979ba9d-da93-4ed1-918c-949350319d7c" />

- **Difficulty Curve:** 배치 순서를 0~1로 정규화해 AnimationCurve 평가, Inspector에서 커브 형태만 바꾸면 코드 수정 없이 난이도 조절 가능
- **Collider Offset 직접 계산:** 에디터 모드에서 `bounds`는 Physics 미계산으로 부정확 → `offset / size / radius` 직접 계산으로 발이 바닥에 정확히 배치
- **ScriptableObject Spawn Rule:** 에셋 교체만으로 즉시 적용, `PrefabUtility.InstantiatePrefab`으로 프리팹 연결 유지
- **Undo 지원:** 전체 배치를 단일 Undo 그룹으로 묶어 `Ctrl+Z` 한 번에 전체 취소

→ [`Editor/EnemyAutoSpawnerEditor.cs`](Assets/Editor/EnemyAutoSpawnerEditor.cs)

<br>

## 트러블슈팅

설계 개선으로 이어진 버그 1건을 기록합니다. 나머지는 각 시스템 섹션의 설계 결정에 통합했습니다.

### 레이어 기반 무적 상태 관리 → bool 플래그 전환

Physics2D 레이어 하나로 여러 무적 상태를 관리하다 보니 무적 상태가 하나 추가될 때마다 `OffDamaged()`의 복원 로직이 그 상태를 알아야 했습니다.      
상태가 2개일 때는 괜찮았지만 피버가 추가되는 순간 의존성이 폭발했습니다.

`isDamageInvincible` / `isUnBeatTime` bool 플래그로 전환해 각 상태가 독립적으로 동작하도록 수정했습니다.      
플래그 방식은 상태 추가 시 기존 로직을 건드리지 않아도 되고 무적 상태 중첩도 자연스럽게 지원됩니다.

<br>

## 프로젝트 구조

```
Assets/Scripts/
├── Player/
│   ├── Core/           # PlayerCoordinator, PlayerStateMachine, WeaponUpgradeManager/Config
│   └── Handlers/       # Transform, Attack, Damage, Shield, Fever, Input
├── Enemies/
│   ├── Base/           # EnemyBase, EnemyStateMachine, IEnemyState
│   ├── Slime/
│   ├── Bat/
│   ├── Piranha/
│   └── FlyingDemon/    # FireBall, FlyingDemon FSM
├── Items/
│   ├── Effects/        # ItemEffectSO 및 Strategy 서브클래스
│   └── Interaction/    # OpenChest 등
├── UI/
│   ├── Components/     # UIButton, StageSelectButton 등
│   ├── HUD/            # SpendableStarsDisplay, HintListUI 등
│   └── Panels/         # WeaponUpgradePanel, SettingManager
├── Puzzle/             # PuzzleGateTrigger, SequenceGate
├── Audio/              # SoundManager
├── Interfaces/         # IShieldBlockable, IAttackHitBox, IHitSoundProvider
├── SaveSystem/         # SaveManager, GameProgress, SaveMigration
└── Stage/

Assets/Editor/
└── EnemyAutoSpawnerEditor.cs
```

<br>

## 실행 방법

1. Unity **2021.3.45f2** 이상에서 프로젝트 열기
2. `Assets/Scenes/` 에서 Title 씬 실행

<br>

---

*Unity Client Developer · 최지현*
