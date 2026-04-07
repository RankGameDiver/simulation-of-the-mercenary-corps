using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MercenaryDetailPanel : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI personalityText;
    [SerializeField] private TextMeshProUGUI statusText; // 행동불능 등 상태 표시

    [Header("기력 / 사기")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TextMeshProUGUI staminaLabel;
    [SerializeField] private Slider moraleSlider;
    [SerializeField] private TextMeshProUGUI moraleLabel;

    [Header("행동 지정")]
    [SerializeField] private TextMeshProUGUI currentActionText;
    [SerializeField] private Transform actionButtonContainer;
    [SerializeField] private GameObject actionButtonPrefab;

    [Header("패널 루트")]
    [SerializeField] private GameObject panelRoot;

    public IMercenary CurrentMercenary { get; private set; }

    // 행동 목록 (나중에 ScriptableObject 등으로 교체 가능)
    private static readonly string[] AvailableActions = { "훈련", "휴식", "독서", "감시", "시장 방문", "기술 연구", "전술 분석" };

    void Start() {
        panelRoot.SetActive(false);
        BuildActionButtons();
    }

    public void Show(IMercenary m) {
        CurrentMercenary = m;
        panelRoot.SetActive(true);
        Render(m);
    }

    public void Hide() {
        CurrentMercenary = null;
        panelRoot.SetActive(false);
    }

    private void Render(IMercenary m) {
        nameText.text = m.Name;
        personalityText.text = $"[{m.PersonalityType}]";

        // 기력
        staminaSlider.value = m.Stamina;
        staminaLabel.text = GetStaminaLabel(m.Stamina);

        // 사기
        moraleSlider.value = m.Morale;
        moraleLabel.text = GetMoraleLabel(m.Morale);

        // 현재 행동
        currentActionText.text = $"현재 행동: {m.CurrentAction}";

        // 상태
        statusText.text = m.IsIncapacitated ? "행동 불능" : "";
        statusText.gameObject.SetActive(m.IsIncapacitated);

        // 행동불능 시 행동 버튼 전체 비활성화
        actionButtonContainer.gameObject.SetActive(!m.IsIncapacitated);
    }

    private void BuildActionButtons() {
        foreach(Transform child in actionButtonContainer) {
            Destroy(child.gameObject);
        }

        foreach(var action in AvailableActions) {
            var go = Instantiate(actionButtonPrefab, actionButtonContainer);
            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<TextMeshProUGUI>();

            label.text = action;
            var captured = action;
            btn.onClick.AddListener(() => AssignAction(captured));
        }
    }

    private void AssignAction(string action) {
        if(CurrentMercenary == null) return;

        CurrentMercenary.AssignAction(action);
        GameEvents.MercenaryStateChanged(CurrentMercenary);
        GameEvents.Log($"[{CurrentMercenary.Name}] 행동 지정: {action}");
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