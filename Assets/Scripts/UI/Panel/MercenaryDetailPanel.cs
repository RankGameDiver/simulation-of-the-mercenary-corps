using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MercenaryDetailPanel : PanelBase
{
    [Header("기본 정보")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("기력")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TextMeshProUGUI staminaLabel;

    [Header("능력치")]
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI agilityText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI charmingText;

    [Header("무장 (선택: 미연결이어도 동작)")]
    [SerializeField] private TextMeshProUGUI weaponText;      // 장착 무기
    [SerializeField] private TextMeshProUGUI gripText;        // 운용 방식
    [SerializeField] private TextMeshProUGUI proficiencyText; // 숙련도

    [Header("행동 지정")]
    [SerializeField] private TextMeshProUGUI currentActionText;
    [SerializeField] private Transform actionButtonContainer;
    [SerializeField] private GameObject actionButtonPrefab;
    [SerializeField] private ActionCatalog actionCatalog; // 지정 가능한 행동 목록의 원천

    public IMercenary CurrentMercenary { get; private set; }

    // 관리 중인 용병의 정보로 패널을 갱신한다 (용병은 한 명뿐이므로 패널은 항상 표시)
    public void Bind(IMercenary m) {
        if(m == null) return;

        CurrentMercenary = m;
        Render(m);
    }

    private void Render(IMercenary m) {
        nameText.text = m.Name;

        // 기력
        staminaSlider.value = m.Stamina;
        staminaLabel.text = GetStaminaLabel(m.Stamina);

        // 능력치
        strengthText.text = $"근력: {m.Strength}";
        agilityText.text = $"민첩: {m.Agility}";
        healthText.text = $"건강: {m.Health}";
        
        intelligenceText.text = $"지능: {m.Intelligence}";
        charmingText.text = $"매력: {m.Charming}";

        // 무장 (무기/운용/숙련도)
        RenderLoadout(m.Loadout);

        // 현재 행동
        currentActionText.text = $"현재 행동: {m.CurrentAction?.DisplayName ?? "대기"}";

        // 용병의 해금 상태에 맞춰 행동 버튼 재구성
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

    // 무기·운용·숙련도 표시 (필드가 연결되지 않았으면 조용히 건너뜀)
    private void RenderLoadout(WeaponLoadout lo) {
        if (weaponText != null)
            weaponText.text = lo?.Weapon != null
                ? $"무기: {lo.Weapon.DisplayName} ({lo.Weapon.WeaponClass.Korean()})"
                : "무기: 없음";

        if (gripText != null)
            gripText.text = lo != null ? $"운용: {lo.Grip.Korean()}" : "운용: -";

        if (proficiencyText != null)
            proficiencyText.text = $"숙련: {BuildProficiencyText(lo)}";
    }

    private static string BuildProficiencyText(WeaponLoadout lo) {
        if (lo == null) return "없음";
        var parts = new System.Collections.Generic.List<string>();
        foreach (Proficiency p in System.Enum.GetValues(typeof(Proficiency))) {
            int level = lo.LevelOf(p);
            if (level > 0) parts.Add($"{p.Korean()} Lv.{level}");
        }
        return parts.Count > 0 ? string.Join(" / ", parts) : "없음";
    }

    // --- 라벨 헬퍼 ---

    private static string GetStaminaLabel(int stamina) => stamina switch {
        <= 5 => "기력: 극심한 피로",
        <= 30 => "기력: 매우 지침",
        <= 60 => "기력: 지침",
        _ => $"기력: {stamina}"
    };
}