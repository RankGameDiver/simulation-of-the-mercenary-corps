using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MercenaryDetailPanel : PanelBase
{
    [Header("기본 정보")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText; // 행동불능 등 상태 표시

    [Header("기력 / 사기")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TextMeshProUGUI staminaLabel;
    [SerializeField] private Slider moraleSlider;
    [SerializeField] private TextMeshProUGUI moraleLabel;

    [Header("능력치")]
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI agilityText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI charmingText;

    [Header("행동 지정")]
    [SerializeField] private TextMeshProUGUI currentActionText;
    [SerializeField] private Transform actionButtonContainer;
    [SerializeField] private GameObject actionButtonPrefab;
    [SerializeField] private ActionCatalog actionCatalog; // 지정 가능한 행동 목록의 원천

    public IMercenary CurrentMercenary { get; private set; }

    protected override void Awake() {
        base.Awake();

        // 선택된 용병이 없으면 시작 시 숨김
        Hide();
    }

    // 특정 용병을 표시하며 패널을 연다
    // (PanelBase.Show()를 오버로드로 가리지 않도록 별도 이름을 사용)
    // 선택이 해제되어 null이 넘어오면 패널을 숨긴다.
    public void ShowMercenary(IMercenary m) {
        if(m == null) {
            Hide();
            return;
        }

        CurrentMercenary = m;
        Render(m);
        Show();
    }

    protected override void OnVisibilityChanged(bool visible) {
        // 숨겨지면 선택 상태도 해제
        if(!visible) {
            CurrentMercenary = null;
        }
    }

    private void Render(IMercenary m) {
        nameText.text = m.Name;

        // 기력
        staminaSlider.value = m.Stamina;
        staminaLabel.text = GetStaminaLabel(m.Stamina);

        // 사기
        moraleSlider.value = m.Morale;
        moraleLabel.text = GetMoraleLabel(m.Morale);

        // 능력치
        strengthText.text = $"근력: {m.Strength}";
        agilityText.text = $"민첩: {m.Agility}";
        healthText.text = $"건강: {m.Health}";
        
        intelligenceText.text = $"지능: {m.Intelligence}";
        charmingText.text = $"매력: {m.Charming}";

        // 현재 행동
        currentActionText.text = $"현재 행동: {m.CurrentAction?.DisplayName ?? "대기"}";

        // 상태
        statusText.text = m.IsIncapacitated ? "행동 불능" : "";
        statusText.gameObject.SetActive(m.IsIncapacitated);

        // 행동불능 시 행동 버튼 전체 비활성화
        actionButtonContainer.gameObject.SetActive(!m.IsIncapacitated);

        // 선택된 용병의 해금 상태에 맞춰 행동 버튼 재구성
        BuildActionButtons(m);
    }

    private void BuildActionButtons(IMercenary m) {
        foreach(Transform child in actionButtonContainer) {
            Destroy(child.gameObject);
        }

        foreach(var action in actionCatalog.Actions) {
            if(action == null) continue;

            bool unlocked = m.IsActionUnlocked(action);
            if(!unlocked && action.HideWhenLocked) continue;

            var go = Instantiate(actionButtonPrefab, actionButtonContainer);
            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<TextMeshProUGUI>();

            label.text = action.DisplayName;
            btn.interactable = unlocked; // 미해금이면 비활성(회색)
            var captured = action;
            btn.onClick.AddListener(() => AssignAction(captured));
        }
    }

    private void AssignAction(ActionDefinition action) {
        if(CurrentMercenary == null) return;

        CurrentMercenary.AssignAction(action);
        GameEvents.MercenaryStateChanged(CurrentMercenary);
        GameEvents.Log($"[{CurrentMercenary.Name}] 행동 지정: {action.DisplayName}");
    }

    // --- 라벨 헬퍼 ---

    private static string GetStaminaLabel(int stamina) => stamina switch {
        <= 5 => "기력: 극심한 피로",
        <= 30 => "기력: 매우 지침",
        <= 60 => "기력: 지침",
        _ => $"기력: {stamina}"
    };

    private static string GetMoraleLabel(int morale) => morale switch {
        >= 70 => "사기: 높음",
        >= 40 => "사기: 보통",
        _ => "사기: 낮음"
    };
}