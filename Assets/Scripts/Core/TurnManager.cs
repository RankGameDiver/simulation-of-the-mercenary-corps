using System.Collections;
using UnityEngine;

public enum TurnPhase { Planning, Execution, Resolution }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private float executionDelay = 0.5f; // 실행 후 딜레이

    public int CurrentTurn { get; private set; }
    public TurnPhase CurrentPhase { get; private set; }

    // 플레이어가 관리하는 단 한 명의 용병 (없으면 null)
    public IMercenary Mercenary { get; private set; }

    // 턴을 넘길 수 있는가: 계획 단계이고, 일과를 직접 지정했을 때만 (기획서 4.1 [확정])
    public bool CanEndPlanning =>
        CurrentPhase == TurnPhase.Planning && Mercenary != null && Mercenary.CurrentAction != null;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable() => GameEvents.OnEndTurnRequested += EndPlanningPhase;
    void OnDisable() => GameEvents.OnEndTurnRequested -= EndPlanningPhase;

    // 게임 시작 (새 게임·이어하기 공통). GameBootstrap이 용병을 준비한 뒤 호출한다.
    // startTurn은 내부 카운터 값(0부터)이다.
    public void BeginGame(IMercenary m, int startTurn = 0) {
        Mercenary = m;
        CurrentTurn = startTurn;
        CurrentPhase = TurnPhase.Planning;

        GameEvents.TurnStarted(CurrentTurn);
        GameEvents.MercenaryStateChanged(Mercenary);
        GameEvents.PlanningPhaseStart();
    }

    // 턴 넘기기 (버튼·키보드 모두 GameEvents.EndTurnRequested를 거쳐 들어온다)
    public void EndPlanningPhase() {
        if(CurrentPhase != TurnPhase.Planning) return;

        // 일과를 고르지 않으면 턴을 넘길 수 없다. 자율 행동은 없다.
        if(!CanEndPlanning) {
            GameEvents.Log("일과를 선택해야 턴을 넘길 수 있습니다.");
            return;
        }

        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn() {
        // 1. 실행 단계
        CurrentPhase = TurnPhase.Execution;
        GameEvents.ExecutionPhaseStart();
        GameEvents.Log($"=== {CurrentTurn + 1}턴 실행 시작 ===");

        Mercenary.OnTurnExecute();
        GameEvents.MercenaryStateChanged(Mercenary);
        GameEvents.Log($"[{Mercenary.Name}] {Mercenary.CurrentAction.DisplayName} 수행");

        yield return new WaitForSeconds(executionDelay);

        // 2. 정산 단계
        CurrentPhase = TurnPhase.Resolution;
        yield return new WaitForSeconds(executionDelay);

        // 3. 다음 턴 준비
        CurrentTurn++;
        CurrentPhase = TurnPhase.Planning;

        Mercenary.OnTurnStart();

        GameEvents.TurnStarted(CurrentTurn);
        GameEvents.PlanningPhaseStart();
        GameEvents.MercenaryStateChanged(Mercenary); // 일과가 비워진 상태를 UI에 반영
        GameEvents.Log($"=== {CurrentTurn}턴 계획 단계 ===");
    }
}
