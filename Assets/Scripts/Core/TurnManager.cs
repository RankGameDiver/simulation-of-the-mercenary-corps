using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnPhase { Planning, Execution, Resolution }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private float executionDelay = 0.5f; // 실행 간 딜레이

    public int CurrentTurn { get; private set; }
    public TurnPhase CurrentPhase { get; private set; }

    private List<IMercenary> _mercenaries = new();

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

    public void RegisterMercenary(IMercenary m) => _mercenaries.Add(m);
    public void UnregisterMercenary(IMercenary m) => _mercenaries.Remove(m);
    public IReadOnlyList<IMercenary> GetMercenaries() => _mercenaries;

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

        foreach(var mercenary in _mercenaries) {
            if(mercenary.IsIncapacitated) {
                GameEvents.Log($"[{mercenary.Name}] 행동불능 상태로 건너뜀");
                continue;
            }

            mercenary.OnTurnExecute();
            GameEvents.MercenaryStateChanged(mercenary);
            GameEvents.Log($"[{mercenary.Name}] {mercenary.CurrentAction?.DisplayName ?? "대기"} 수행");

            yield return new WaitForSeconds(executionDelay);
        }

        // 2. 정산 단계
        CurrentPhase = TurnPhase.Resolution;
        yield return new WaitForSeconds(executionDelay);

        // 3. 다음 턴 준비
        CurrentTurn++;
        CurrentPhase = TurnPhase.Planning;

        foreach(var mercenary in _mercenaries) {
            mercenary.OnTurnStart();
        }

        GameEvents.TurnStarted(CurrentTurn);
        GameEvents.PlanningPhaseStart();
        GameEvents.Log($"=== {CurrentTurn}턴 계획 단계 ===");
    }
}