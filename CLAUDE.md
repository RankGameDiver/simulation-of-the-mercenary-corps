# CLAUDE.md

이 파일은 이 저장소에서 작업하는 Claude Code(claude.ai/code)에게 지침을 제공합니다.

## 프로젝트 개요

Unity로 제작 중인 턴제 용병단 운영 시뮬레이션입니다. 프로젝트는 초기 개발 단계이며, `Assets/Scripts/Core/` 밖의 클래스 대부분은 아직 스텁 상태입니다. 게임 내용은 한국어로 작성됩니다(주석, 문자열 리터럴, 필드 라벨 모두 한국어).

## Unity 설정

- **엔진**: Unity 6 (URP 17.3.0)
- **렌더 파이프라인**: Universal Render Pipeline
- **주요 패키지**: Input System, Unity Test Framework, AI Navigation, Timeline, uGUI 2.0

프로젝트는 Unity 에디터에서 열고 실행합니다 — 개발용 CLI 빌드 명령은 없습니다. 테스트는 Unity의 **Test Runner** 창(`Window > General > Test Runner`)에서 실행합니다.

## 아키텍처

### 이벤트 버스 (`Assets/Scripts/Core/GameEvents.cs`)
`GameEvents`는 전역 이벤트 버스 역할을 하는 static 클래스입니다. 시스템 간 통신(턴 페이즈 변경, 용병 상태 갱신, 로그 메시지)은 모두 이곳을 거칩니다. 각 시스템은 서로를 직접 참조하지 않고 이벤트를 구독합니다.

### 턴 흐름 (`Assets/Scripts/Core/TurnManager.cs`)
`TurnManager`는 세 단계 루프를 관리하는 싱글톤 `MonoBehaviour`입니다:
1. **계획** (`TurnPhase.Planning`) — UI가 용병에게 행동을 지정합니다. `EndPlanningPhase()` 호출로 종료되며, 이 메서드는 UI 버튼에 연결되어 있습니다
2. **실행** (`TurnPhase.Execution`) — 등록된 용병을 순회하며 각각의 `OnTurnExecute()`를 호출하고, 사이마다 설정 가능한 `executionDelay` 만큼 대기합니다
3. **정산** (`TurnPhase.Resolution`) — 잠시 대기한 뒤 턴 카운터를 올리고 다시 계획 단계로 돌아갑니다

### 용병 추상화 (`Assets/Scripts/Core/IMercenary.cs`)
`IMercenary`는 데이터 계층과 턴/UI 시스템 사이의 유일한 인터페이스입니다. 새로운 용병 데이터 구현을 추가할 때는 이 인터페이스를 구현하세요. 턴과 UI 시스템은 구체 타입이 아니라 반드시 `IMercenary`에만 의존해야 합니다.

핵심 계약:
- `AssignAction(ActionDefinition action)` — 계획 단계에서 UI가 호출
- `OnTurnExecute()` — 실행 단계에서 `TurnManager`가 호출
- `IsIncapacitated` — `true`이면 `TurnManager`가 해당 용병을 건너뜀

### 용병 데이터 (`Assets/Scripts/Mercenary/DummyMercenaryData.cs`)
`DummyMercenaryData`는 프로토타이핑에 쓰이는 현재의 `IMercenary` 구현체입니다. 플레이어가 행동을 지정하지 않으면 기본 자율 행동(`"휴식"`)을 수행합니다. 스태미나는 매 턴 10씩 감소하고, `Stamina <= 0`이 되면 `IsIncapacitated`가 발동합니다.

## 폰트 (`Assets/Scripts/Editor/StaticFontAssetBaker.cs`)

프로젝트의 모든 TMP 폰트 애셋은 **Static** 아틀라스(`m_AtlasPopulationMode: 0`)로 베이크되어 있습니다. 이는 의도된 설정입니다. Dynamic 모드에서는 어떤 문자가 처음 렌더링될 때 TMP가 아틀라스에 글리프를 덧붙이고 `.asset` 파일을 다시 쓰기 때문에, 단순히 플레이 모드에 들어가기만 해도 수 메가바이트짜리 diff가 생깁니다. 한국어에서는 이 문제가 상시적입니다 — 완성형 음절이 11,172자라 새 텍스트마다 새 글리프가 계속 추가됩니다.

`Assets/Fonts/NotoSansKR SDF.asset`이 프로젝트 폰트이며(48pt, padding 5, SDFAA, 4096² × 3 아틀라스), 완성형 한글 전체와 ASCII, 자모, 자주 쓰는 문장부호·기호를 포함합니다.

폰트, 샘플링 크기, 문자 집합을 바꾸려면 `StaticFontAssetBaker.cs` 상단의 상수를 수정한 뒤 **`Tools > Font > Static SDF 베이크`**를 실행하세요. Font Asset Creator로 애셋을 제자리에서 재생성하지 마세요.

**`AssetDatabase.CreateAsset`으로 폰트 애셋을 다시 만들지 마세요.** 씬과 프리팹은 폰트의 *머티리얼 서브애셋*을 fileID로 참조하는데(NotoSansKR의 경우 `-7482567356781329147`), 애셋을 재생성하면 이 ID가 바뀌어 모든 텍스트 오브젝트가 분홍색으로 변합니다. 베이커는 대신 메모리에서 폰트 애셋을 만든 뒤 `EditorUtility.CopySerialized`로 옮겨 심어서, 머티리얼과 메인 애셋의 GUID를 보존합니다.

## 주요 규칙

- 시스템 간 통신은 모두 `GameEvents`를 사용합니다. 시스템끼리 MonoBehaviour를 직접 참조하지 마세요.
- 모든 TMP 폰트 애셋을 Static으로 유지하세요. Font Asset Creator는 새 애셋을 Dynamic으로 만들기 때문에, 그대로 두면 에디터를 실행할 때마다 저장소가 지저분해집니다. TMP 패키지 자체의 `LiberationSans SDF - Fallback`도 포함됩니다.
- 새로운 용병 타입은 모두 `IMercenary`를 구현하고 `TurnManager`에 등록/해제해야 합니다(`RegisterMercenary` / `UnregisterMercenary`).
- `MercenaryData.cs`와 `UIManager.cs`는 빈 스텁입니다 — 프로젝트가 커지면서 구현해 나가세요.
- 용병 행동은 `ActionDefinition` ScriptableObject로 정의합니다(`Assets/Scripts/Actions/`). 지정 가능한 행동 목록은 `ActionCatalog` 애셋 하나가 단일 원천이며, 하드코딩 문자열 배열을 다시 도입하지 마세요. 새 행동은 `Create > 용병단 > 행동 정의`로 애셋을 만들어 카탈로그에 추가합니다. `IMercenary.CurrentAction`이 `null`이면 미지정(대기) 상태입니다.
- 행동은 용병별로 해금됩니다. 각 `ActionDefinition`의 `unlockChance`(0~1)에 따라 용병 생성 시(`DummyMercenaryData`) 확률적으로 `unlockedActions` 집합이 채워집니다(`1`=항상 해금, `0`=해금 안 됨). 수행 가능 여부는 `IMercenary.IsActionUnlocked`로 판정하세요. 이 확률 방식은 프로토타이핑용이며, 향후 조건부 해금(요구 스탯 등)이 필요하면 별도 판별자를 도입할 예정입니다. 미해금 행동을 상세 패널에서 숨길지(`hideWhenLocked=true`) 비활성 회색으로 보일지(`false`)는 각 `ActionDefinition` 애셋에서 행동별로 설정합니다.
