using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("패널 참조")]
    [SerializeField] private MercenaryDetailPanel mercenaryDetailPanel;
    [SerializeField] private ActionLogPanel logPanel;
    [SerializeField] private ResourceBar resourceBar;

    [Header("턴 컨트롤")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI turnPhaseText;
    [SerializeField] private TextMeshProUGUI turnNumberText;

    void Start() {
        endTurnButton.onClick.AddListener(() => TurnManager.Instance.EndPlanningPhase());
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
        turnNumberText.text = $"Turn {turn}";

        // 용병 생성(GameBootstrap)과 턴 시작의 Start 순서는 보장되지 않으므로 있을 때만 갱신
        var m = TurnManager.Instance.Mercenary;
        if(m != null) mercenaryDetailPanel.Bind(m);
    }

    private void OnPlanningPhase() {
        turnPhaseText.text = "[ 계획 단계 ]";
        endTurnButton.interactable = true;
    }

    private void OnExecutionPhase() {
        turnPhaseText.text = "[ 실행 중... ]";
        endTurnButton.interactable = false; // 실행 중 버튼 비활성화
    }

    private void OnMercenaryStateChanged(IMercenary m) => mercenaryDetailPanel.Bind(m);
}
