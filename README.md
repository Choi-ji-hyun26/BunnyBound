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
| 개발 기간 | 2026.04.23 – 2026.05 |
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

### 2. 적 AI — EnemyBase 상속 구조

피격·넉백·스턴·사망의 공통 흐름을 `EnemyBase`에서 처리하고 각 적은 고유 상태만 정의합니다.

| | 슬라임 | 박쥐 | 피라냐 |
|---|---|---|---|
| 이동 | 지상 패트롤 | 공중 | 고정 |
| 상태 수 | 1 | 4 | 3 |
| 감지 | — | Raycast 시야 | 근접 상방 반원 Raycast |
| 실드 반응 | 넉백 | 넉백 | 스턴 |

**Bat FSM 상태 전이:**

```
[Patrol] ──감지──▶ [Dash] ──충돌──▶ [Bounce]
▲                  │                 │
│                미감지           재감지 → [Dash]
│                  ▼            미감지 ↓
└──────────── [Return] ◀─────────────┘
```

**`IsForceDash` — 버그 수정에서 설계로:**      
`DashState`가 피격 후 진입해도 감지 범위 밖이면 첫 프레임에 즉시 ReturnState로 전환되는 문제가 있었습니다.     
`IsForceDash` 플래그와 `LastKnownPlayerPosition`을 추가해 원거리 공격 시 Bat이 마지막 플레이어 위치로 추격하는 자연스러운 적 반응으로 발전했습니다.

**단일 피격 진입점 — `PlayerHurtBox.HandleHit()`:**     
`EnemyHitBox`가 `PlayerDamageHandler.OnDamaged()`를 직접 호출하면서 `hitCooldown` 체크가 우회됐습니다.     
모든 피격 경로를 `PlayerHurtBox.HandleHit()` 단일 진입점으로 일원화해 cooldown이 항상 적용되도록 수정했습니다.

**Slime — ThinkRoutine 분리:**     
의사결정(`ThinkRoutine` 코루틴)과 물리 실행(`Update`)을 분리해 `Update`는 velocity 적용과 엣지/벽 감지만 담당합니다.    
 
**Piranha — 애니메이션 이벤트 공격 타이밍:**      
`WaitForSeconds` 대신 Animation Event로 공격 콜라이더를 제어합니다. 클립 speed가 변경되어도 판정 타이밍이 항상 애니메이션에 동기화됩니다.

→ [`Enemies/`](Assets/Scripts/Enemies/)

<br>

### 3. 세이브 시스템

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

→ [`SaveSystem/`](Assets/Scripts/SaveSystem/)

<br>

### 4. 에디터 툴 — 난이도 커브 기반 적 자동 배치

Tilemap을 스캔해 유효 바닥 위치를 수집하고 AnimationCurve 기반 난이도 곡선으로 적을 자동 배치합니다.       
수동 배치 대비 작업 시간 **약 30% 단축**되었습니다.

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
├── Editor/
│   └── EnemyAutoSpawnerEditor.cs
└── ScriptableObjects/
└── EnemySpawnRule
```

<br>

## 실행 방법

1. Unity **2021.3.45f2** 이상에서 프로젝트 열기
2. `Assets/Scenes/` 에서 Title 씬 실행

<br>

## 링크

YouTube 게임 소개 영상
https://youtu.be/dB3QyTMR4tI

---

*Unity Client Developer · 최지현*
