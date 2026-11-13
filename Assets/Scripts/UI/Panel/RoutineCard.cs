using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 일과 카드 한 장 (RoutineCard 프리팹의 루트)
// 카드를 누르면 일과가 "지정"만 된다. 실행은 턴을 넘길 때.
public class RoutineCard : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private Outline selectedOutline; // 선택됨 테두리
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hotkeyText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI stateText; // 선택됨 / 미해금 등 상태 문구

    [Header("색")]
    [SerializeField] private Color normalColor = new(0.17f, 0.19f, 0.23f);
    [SerializeField] private Color selectedColor = new(0.30f, 0.26f, 0.15f);
    [SerializeField] private Color lockedColor = new(0.12f, 0.13f, 0.15f);
    [SerializeField] private Color normalTextColor = new(0.92f, 0.92f, 0.92f);
    [SerializeField] private Color lockedTextColor = new(0.45f, 0.45f, 0.48f);

    public ActionDefinition Action { get; private set; }

    public void Setup(ActionDefinition action, int hotkeyNumber, Action<ActionDefinition> onClick) {
        Action = action;
        nameText.text = action.DisplayName;
        hotkeyText.text = hotkeyNumber is >= 1 and <= 9 ? $"[{hotkeyNumber}]" : "";
        descriptionText.text = string.IsNullOrWhiteSpace(action.Description) ? "" : action.Description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(Action));
    }

    // unlocked: 해금 여부 / selected: 이번 턴 지정된 일과 / canSelect: 지금 고를 수 있는 단계인가(계획 단계)
    public void SetState(bool unlocked, bool selected, bool canSelect) {
        button.interactable = unlocked && canSelect;

        background.color = !unlocked ? lockedColor : selected ? selectedColor : normalColor;
        if(selectedOutline != null) selectedOutline.enabled = selected;

        var textColor = unlocked ? normalTextColor : lockedTextColor;
        nameText.color = textColor;
        descriptionText.color = textColor;

        stateText.text = !unlocked ? "미해금" : selected ? "● 선택됨" : "";
    }
}
