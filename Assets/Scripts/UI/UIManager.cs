using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 육성 화면의 턴 표시와 [턴 넘기기] 버튼을 관리한다.
// 각 패널(상태·일과·성장·로그)은 GameEvents를 직접 구독해 스스로 갱신한다.
public class UIManager : MonoBehaviour
{
    [Header("상단 바")]
    [SerializeField] private TextMeshProUGUI turnInfoText; // "1턴 · 계획 단계"

    [Header("턴 넘기기")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI endTurnHintText; // 버튼 옆 안내 ("일과를 선택하세요")

    private int _turnNumber = 1;
    private string _phaseLabel = "계획 단계";
    private bool _planning = true;
    private bool _hasRoutine;

    void Start() {
        endTurnButton.onClick.AddListener(GameEvents.EndTurnRequested);
        Render();
    }

    void OnEnable() {
        GameEvents.OnTurnStarted += OnTurnStarted;
        GameEvents.OnPlanningPhaseStart += OnPlanningPhase;
        GameEvents.OnExecutionPhaseStart += OnExecutionPhase;
        GameEvents.OnMercenaryStateChanged += OnMercenaryStateChanged;
    }

    void OnDisable() {
        GameEvents.OnTurnStarted -= OnTurnStarted;
        GameEvents.OnPlanningPhaseStart -= OnPlanningPhase;
        GameEvents.OnExecutionPhaseStart -= OnExecutionPhase;
        GameEvents.OnMercenaryStateChanged -= OnMercenaryStateChanged;
    }

    private void OnTurnStarted(int turn) {
        _turnNumber = turn + 1; // 내부 카운터는 0부터, 표시는 1턴부터
        Render();
    }

    private void OnPlanningPhase() {
        _planning = true;
        _phaseLabel = "계획 단계";
        Render();
    }

    private void OnExecutionPhase() {
        _planning = false;
        _phaseLabel = "실행 중…";
        Render();
    }

    private void OnMercenaryStateChanged(IMercenary m) {
        _hasRoutine = m != null && m.CurrentAction != null;
        Render();
    }

    private void Render() {
        if(turnInfoText != null) turnInfoText.text = $"{_turnNumber}턴 · {_phaseLabel}";

        // 일과를 직접 고른 계획 단계에서만 턴을 넘길 수 있다 (기획서 4.1 [확정])
        if(endTurnButton != null) endTurnButton.interactable = _planning && _hasRoutine;
        if(endTurnHintText != null) {
            endTurnHintText.text = !_planning ? "실행 중…"
                                 : _hasRoutine ? "[Space] 턴 넘기기"
                                 : "일과를 선택하세요";
        }
    }
}
