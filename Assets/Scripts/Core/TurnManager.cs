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

    void Awake() {
        if (Instance != null) { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
    }

    void Start() {
        CurrentPhase = TurnPhase.Planning;
        GameEvents.TurnStarted(CurrentTurn);
        GameEvents.PlanningPhaseStart();
    }

    public void SetMercenary(IMercenary m) => Mercenary = m;

    // UI의 턴 종료 버튼에서 호출
    public void EndPlanningPhase() {
        if(CurrentPhase != TurnPhase.Planning) return;
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn() {
        // 1. 실행 단계
        CurrentPhase = TurnPhase.Execution;
        GameEvents.ExecutionPhaseStart();
        GameEvents.Log($"=== {CurrentTurn + 1}턴 실행 시작 ===");

        if(Mercenary != null) {
            Mercenary.OnTurnExecute();
            GameEvents.MercenaryStateChanged(Mercenary);
            GameEvents.Log($"[{Mercenary.Name}] {Mercenary.CurrentAction?.DisplayName ?? "대기"} 수행");

            yield return new WaitForSeconds(executionDelay);
        }

        // 2. 정산 단계
        CurrentPhase = TurnPhase.Resolution;
        yield return new WaitForSeconds(executionDelay);

        // 3. 다음 턴 준비
        CurrentTurn++;
        CurrentPhase = TurnPhase.Planning;

        Mercenary?.OnTurnStart();

        GameEvents.TurnStarted(CurrentTurn);
        GameEvents.PlanningPhaseStart();
        GameEvents.Log($"=== {CurrentTurn}턴 계획 단계 ===");
    }
}
