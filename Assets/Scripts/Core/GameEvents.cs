using System;

public static class GameEvents
{
    // 턴 관련
    public static event Action<int> OnTurnStarted;
    public static event Action OnPlanningPhaseStart;
    public static event Action OnExecutionPhaseStart;
    public static event Action OnTurnEnded;

    // 용병 관련
    public static event Action<IMercenary> OnMercenaryStateChanged;

    // 입력 요청 (버튼·키보드 → 각 시스템). 요청을 받은 쪽이 가능 여부를 판단한다.
    public static event Action OnEndTurnRequested;            // 턴 넘기기
    public static event Action<int> OnRoutineHotkeyPressed;   // 일과 단축키 (0부터 시작하는 카탈로그 순서)
    public static event Action OnGrowthTabCycleRequested;     // 성장 정보 탭 전환
    public static event Action OnMenuRequested;               // Esc: 열린 창 닫기, 없으면 게임 메뉴 열기

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
    public static void MercenaryStateChanged(IMercenary m) => OnMercenaryStateChanged?.Invoke(m);
    public static void EndTurnRequested() => OnEndTurnRequested?.Invoke();
    public static void RoutineHotkeyPressed(int index) => OnRoutineHotkeyPressed?.Invoke(index);
    public static void GrowthTabCycleRequested() => OnGrowthTabCycleRequested?.Invoke();
    public static void MenuRequested() => OnMenuRequested?.Invoke();
    public static void CombatStarted() => OnCombatStarted?.Invoke();
    public static void CombatEnded(bool playerWon) => OnCombatEnded?.Invoke(playerWon);
    public static void CombatantActed(ICombatant c) => OnCombatantActed?.Invoke(c);
    public static void Log(string msg) => OnLogMessage?.Invoke(msg);
}
