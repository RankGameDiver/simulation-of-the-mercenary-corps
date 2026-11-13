using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 육성 화면 가운데 열: 일과 선택
// - 일과는 플레이어가 직접 고른다. 카드를 누르면 지정만 되고, 실행은 턴을 넘길 때 (기획서 4.1 [확정])
// - 턴을 넘기기 전에는 몇 번이든 바꿀 수 있다
// - 카드 목록의 원천은 ActionCatalog 하나뿐이다
public class RoutinePanel : PanelBase
{
    [Header("데이터")]
    [SerializeField] private ActionCatalog actionCatalog;

    [Header("참조")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private RoutineCard cardPrefab;
    [SerializeField] private TextMeshProUGUI selectionText; // 선택 요약 (예상 변화 미리보기 자리)

    private readonly List<RoutineCard> _cards = new();
    private IMercenary _mercenary;
    private bool _planning = true; // 계획 단계에서만 고를 수 있다

    void OnEnable() {
        GameEvents.OnMercenaryStateChanged += Bind;
        GameEvents.OnPlanningPhaseStart += OnPlanningPhase;
        GameEvents.OnExecutionPhaseStart += OnExecutionPhase;
        GameEvents.OnRoutineHotkeyPressed += OnHotkey;
    }

    void OnDisable() {
        GameEvents.OnMercenaryStateChanged -= Bind;
        GameEvents.OnPlanningPhaseStart -= OnPlanningPhase;
        GameEvents.OnExecutionPhaseStart -= OnExecutionPhase;
        GameEvents.OnRoutineHotkeyPressed -= OnHotkey;
    }

    public void Bind(IMercenary m) {
        if(m == null) return;

        // 용병이 바뀌면(해금 집합이 달라지므로) 카드를 다시 만든다
        if(!ReferenceEquals(m, _mercenary)) {
            _mercenary = m;
            BuildCards();
        }

        Refresh();
    }

    private void OnPlanningPhase() { _planning = true; Refresh(); }
    private void OnExecutionPhase() { _planning = false; Refresh(); }

    // 단축키는 카탈로그 순서(0부터)를 따른다
    private void OnHotkey(int index) {
        if(actionCatalog == null || index < 0 || index >= actionCatalog.Actions.Count) return;
        Select(actionCatalog.Actions[index]);
    }

    private void BuildCards() {
        foreach(var card in _cards) {
            if(card != null) Destroy(card.gameObject);
        }
        _cards.Clear();

        if(actionCatalog == null || cardPrefab == null) {
            Debug.LogError("[RoutinePanel] 행동 카탈로그 또는 카드 프리팹이 연결되지 않았습니다.");
            return;
        }

        for(int i = 0; i < actionCatalog.Actions.Count; i++) {
            var action = actionCatalog.Actions[i];
            if(action == null) continue;
            if(!_mercenary.IsActionUnlocked(action) && action.HideWhenLocked) continue;

            var card = Instantiate(cardPrefab, cardContainer);
            card.Setup(action, i + 1, Select);
            _cards.Add(card);
        }
    }

    private void Refresh() {
        if(_mercenary == null) return;

        foreach(var card in _cards) {
            bool unlocked = _mercenary.IsActionUnlocked(card.Action);
            bool selected = card.Action == _mercenary.CurrentAction;
            card.SetState(unlocked, selected, _planning);
        }

        if(selectionText != null) {
            selectionText.text = _mercenary.CurrentAction != null
                ? $"이번 턴 일과: <b>{_mercenary.CurrentAction.DisplayName}</b>  —  턴을 넘기면 실행됩니다"
                : "일과를 선택하세요";
        }
    }

    private void Select(ActionDefinition action) {
        if(!_planning || _mercenary == null || action == null) return;
        if(!_mercenary.IsActionUnlocked(action)) return;
        if(_mercenary.CurrentAction == action) return; // 이미 선택된 일과

        _mercenary.AssignAction(action);
        GameEvents.MercenaryStateChanged(_mercenary);
        GameEvents.Log($"[{_mercenary.Name}] 일과 지정: {action.DisplayName}");
    }
}
