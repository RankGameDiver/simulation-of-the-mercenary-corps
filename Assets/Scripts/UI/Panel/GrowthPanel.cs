using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 육성 화면 오른쪽 열: 성장 정보 (탭 3개: 숙련 / 스킬 / 장비)
// 1단계 구현: 숙련은 레벨만, 스킬은 자리만, 장비는 무기·운용·상성 고리 열람.
// 숙련 경험치·스킬 시스템이 생기면 각 탭을 전용 뷰로 교체한다 (Docs/미구현_목록.md 9장 3단계).
public class GrowthPanel : PanelBase
{
    public enum Tab { Proficiency = 0, Skill = 1, Equipment = 2 }

    [Serializable]
    public class TabEntry
    {
        public Button button;
        public Image buttonBackground;
        public GameObject content;
    }

    [Header("탭 (숙련, 스킬, 장비 순서)")]
    [SerializeField] private TabEntry[] tabs = new TabEntry[3];

    [Header("탭 내용")]
    [SerializeField] private TextMeshProUGUI proficiencyText;
    [SerializeField] private TextMeshProUGUI skillText;
    [SerializeField] private TextMeshProUGUI equipmentText;

    [Header("색")]
    [SerializeField] private Color activeTabColor = new(0.30f, 0.26f, 0.15f);
    [SerializeField] private Color inactiveTabColor = new(0.17f, 0.19f, 0.23f);

    private Tab _current = Tab.Proficiency;

    protected override void Awake() {
        base.Awake();
        for(int i = 0; i < tabs.Length; i++) {
            int index = i;
            if(tabs[i]?.button != null) tabs[i].button.onClick.AddListener(() => SelectTab((Tab)index));
        }
        SelectTab(_current);
    }

    void OnEnable() {
        GameEvents.OnMercenaryStateChanged += Bind;
        GameEvents.OnGrowthTabCycleRequested += CycleTab;
    }

    void OnDisable() {
        GameEvents.OnMercenaryStateChanged -= Bind;
        GameEvents.OnGrowthTabCycleRequested -= CycleTab;
    }

    public void SelectTab(Tab tab) {
        _current = tab;
        for(int i = 0; i < tabs.Length; i++) {
            if(tabs[i] == null) continue;
            bool active = i == (int)tab;
            if(tabs[i].content != null) tabs[i].content.SetActive(active);
            if(tabs[i].buttonBackground != null) tabs[i].buttonBackground.color = active ? activeTabColor : inactiveTabColor;
        }
    }

    private void CycleTab() => SelectTab((Tab)(((int)_current + 1) % tabs.Length));

    public void Bind(IMercenary m) {
        if(m == null) return;
        if(proficiencyText != null) proficiencyText.text = BuildProficiency(m.Loadout);
        if(skillText != null) skillText.text = "장착 (0/3)\n[ 비어 있음 ]  [ 비어 있음 ]  [ 비어 있음 ]\n\n<color=#888>스킬 시스템 구현 후 표시됩니다.</color>";
        if(equipmentText != null) equipmentText.text = BuildEquipment(m.Loadout);
    }

    // 계열 숙련 4종. 장착 무기의 지배 숙련(공격으로 오르는 숙련)은 강조한다.
    private static string BuildProficiency(WeaponLoadout lo) {
        var sb = new StringBuilder();
        var governing = lo?.Weapon != null ? lo.Weapon.GoverningProficiency : (Proficiency?)null;

        foreach(Proficiency p in Enum.GetValues(typeof(Proficiency))) {
            int level = lo != null ? lo.LevelOf(p) : 0;
            string line = $"{p.Korean()}<pos=35%>Lv.{level}";
            sb.AppendLine(p == governing ? $"<color=#E0B050>{line}  ◀ 주무기</color>" : line);
        }

        sb.AppendLine();
        sb.AppendLine("<color=#888>다음 해금</color>");
        if(lo?.Weapon != null) {
            int lv = lo.WeaponLevel;
            string prof = lo.Weapon.GoverningProficiency.Korean();
            if(lv < 3) sb.AppendLine($"· {prof} Lv.3  양손 운용");
            if(lv < 5) sb.AppendLine($"· {prof} Lv.5  필살기");
            if(lv >= 5) sb.AppendLine("· 모두 해금됨");
        } else {
            sb.AppendLine("· 무기 없음");
        }

        sb.AppendLine();
        sb.Append("<color=#888>경험치 막대는 숙련 경험치 시스템 구현 후 표시됩니다.</color>");
        return sb.ToString();
    }

    private static string BuildEquipment(WeaponLoadout lo) {
        if(lo?.Weapon == null) return "무기 없음";

        var w = lo.Weapon;
        var sb = new StringBuilder();
        sb.AppendLine($"<b>{w.DisplayName}</b>");
        sb.AppendLine($"계열  {w.WeaponClass.Korean()}  ·  지배 숙련  {w.GoverningProficiency.Korean()}");
        sb.AppendLine($"운용  {lo.Grip.Korean()}");
        if(!string.IsNullOrWhiteSpace(w.Description)) sb.AppendLine(w.Description);

        // 상성 고리: 창 > 검 > 둔기 > 창 (내 계열 강조)
        sb.AppendLine();
        sb.AppendLine("<color=#888>상성</color>");
        string Mark(WeaponClass c) => c == w.WeaponClass ? $"<color=#E0B050><b>{c.Korean()}</b></color>" : c.Korean();
        sb.Append($"{Mark(WeaponClass.Spear)} > {Mark(WeaponClass.Sword)} > {Mark(WeaponClass.Blunt)} > {Mark(WeaponClass.Spear)}");
        return sb.ToString();
    }
}
