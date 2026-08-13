using System;

public static class GameEvents
{
    // 턴 관련
    public static event Action<int> OnTurnStarted;
    public static event Action OnPlanningPhaseStart;
    public static event Action OnExecutionPhaseStart;
    public static event Action OnTurnEnded;

    // 용병 관련
    public static event Action<IMercenary> OnMercenarySelected;
    public static event Action<IMercenary> OnMercenaryStateChanged;

    // 전투 관련
    public static event Action OnCombatStarted;
    public static event Action<bool> OnCombatEnded; // true=아군 승리
    public static event Action<ICombatant> OnCombatantActed;

    // 로그
    public static event Action<string> OnLogMessage;

    // 호출 메서드
    public static void TurnStarted(int turn) => OnTurnStarted?.Invoke(turn);
    public static void PlanningPhaseStart() => OnPlanningPhaseStart?.Invoke();
    public static void ExecutionPhaseStart() => OnExecutionPhaseStart?.Invoke();
    public static void TurnEnded() => OnTurnEnded?.Invoke();
    public static void SelectMercenary(IMercenary m) => OnMercenarySelected?.Invoke(m);
    public static void MercenaryStateChanged(IMercenary m) => OnMercenaryStateChanged?.Invoke(m);
    public static void CombatStarted() => OnCombatStarted?.Invoke();
    public static void CombatEnded(bool playerWon) => OnCombatEnded?.Invoke(playerWon);
    public static void CombatantActed(ICombatant c) => OnCombatantActed?.Invoke(c);
    public static void Log(string msg) => OnLogMessage?.Invoke(msg);
}