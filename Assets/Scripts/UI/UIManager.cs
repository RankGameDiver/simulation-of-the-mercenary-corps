using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("패널 참조")]
    [SerializeField] private MercenaryListPanel mercenaryListPanel;
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
        GameEvents.OnMercenarySelected += OnMercenarySelected;
        GameEvents.OnMercenaryStateChanged += OnMercenaryStateChanged;
    }

    void OnDisable() {
        GameEvents.OnTurnStarted -= OnTurnStarted;
        GameEvents.OnPlanningPhaseStart -= OnPlanningPhase;
        GameEvents.OnExecutionPhaseStart -= OnExecutionPhase;
        GameEvents.OnMercenarySelected -= OnMercenarySelected;
        GameEvents.OnMercenaryStateChanged -= OnMercenaryStateChanged;
    }

    private void OnTurnStarted(int turn) {
        turnNumberText.text = $"Turn {turn}";
        mercenaryListPanel.Refresh(TurnManager.Instance.GetMercenaries());
    }

    private void OnPlanningPhase() {
        turnPhaseText.text = "[ 계획 단계 ]";
        endTurnButton.interactable = true;
    }

    private void OnExecutionPhase() {
        turnPhaseText.text = "[ 실행 중... ]";
        endTurnButton.interactable = false; // 실행 중 버튼 비활성화
    }

    private void OnMercenarySelected(IMercenary m) => mercenaryDetailPanel.Show(m);
    private void OnMercenaryStateChanged(IMercenary m) {
        mercenaryListPanel.RefreshItem(m);
        if(mercenaryDetailPanel.CurrentMercenary?.Id == m.Id) {
            mercenaryDetailPanel.Show(m);
        }
    }
}