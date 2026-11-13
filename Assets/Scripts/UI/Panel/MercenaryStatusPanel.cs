using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 육성 화면 왼쪽 열: 용병 상태 (읽기 전용, 항상 표시)
// 정체성 → 소모 자원(기력·HP) → 능력치 → 무장 순서로 보여준다. 조작은 없다.
public class MercenaryStatusPanel : PanelBase
{
    [Serializable]
    public class StatRow
    {
        public Slider bar;              // 1–10 막대 (조작 불가)
        public TextMeshProUGUI value;   // 숫자
    }

    public const int StatMax = 10;

    [Header("정체성")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("기력")]
    [SerializeField] private Slider staminaBar;
    [SerializeField] private TextMeshProUGUI staminaText;

    [Header("HP (전투 후에도 유지)")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField, Range(0f, 1f)] private float lowHpRatio = 0.3f; // 이 비율 이하면 위험 색

    [Header("능력치")]
    [SerializeField] private StatRow strength = new();
    [SerializeField] private StatRow agility = new();
    [SerializeField] private StatRow health = new();
    [SerializeField] private StatRow intelligence = new();
    [SerializeField] private StatRow charming = new();

    [Header("무장")]
    [SerializeField] private TextMeshProUGUI weaponText;
    [SerializeField] private TextMeshProUGUI gripText;

    [Header("색")]
    [SerializeField] private Color normalColor = new(0.42f, 0.70f, 0.45f);
    [SerializeField] private Color warnColor = new(0.85f, 0.65f, 0.25f);
    [SerializeField] private Color dangerColor = new(0.82f, 0.30f, 0.27f);

    void OnEnable() => GameEvents.OnMercenaryStateChanged += Bind;
    void OnDisable() => GameEvents.OnMercenaryStateChanged -= Bind;

    public void Bind(IMercenary m) {
        if(m == null) return;

        nameText.text = m.Name;

        // 기력
        SetBar(staminaBar, m.Stamina, 100);
        staminaText.text = $"기력 {m.Stamina} · {GetStaminaLabel(m.Stamina)}";
        SetFillColor(staminaBar, m.Stamina <= 30 ? dangerColor : m.Stamina <= 60 ? warnColor : normalColor);

        // HP
        SetBar(hpBar, m.CurrentHp, Mathf.Max(1, m.MaxHp));
        hpText.text = $"HP {m.CurrentHp} / {m.MaxHp}";
        bool lowHp = m.MaxHp > 0 && m.CurrentHp <= m.MaxHp * lowHpRatio;
        SetFillColor(hpBar, lowHp ? dangerColor : normalColor);

        // 능력치
        SetStat(strength, m.Strength);
        SetStat(agility, m.Agility);
        SetStat(health, m.Health);
        SetStat(intelligence, m.Intelligence);
        SetStat(charming, m.Charming);

        // 무장 (상세는 성장 정보의 장비 탭)
        var lo = m.Loadout;
        if(weaponText != null)
            weaponText.text = lo?.Weapon != null
                ? $"무기  {lo.Weapon.DisplayName} ({lo.Weapon.WeaponClass.Korean()})"
                : "무기  없음";
        if(gripText != null)
            gripText.text = lo != null ? $"운용  {lo.Grip.Korean()}" : "운용  -";
    }

    private static void SetStat(StatRow row, int value) {
        if(row == null) return;
        SetBar(row.bar, value, StatMax);
        if(row.value != null) row.value.text = value.ToString();
    }

    private static void SetBar(Slider bar, int value, int max) {
        if(bar == null) return;
        bar.minValue = 0;
        bar.maxValue = max;
        bar.value = Mathf.Clamp(value, 0, max);
    }

    private static void SetFillColor(Slider bar, Color color) {
        if(bar != null && bar.fillRect != null && bar.fillRect.TryGetComponent(out Image fill))
            fill.color = color;
    }

    private static string GetStaminaLabel(int stamina) => stamina switch {
        <= 5 => "극심한 피로",
        <= 30 => "매우 지침",
        <= 60 => "지침",
        _ => "양호"
    };
}
