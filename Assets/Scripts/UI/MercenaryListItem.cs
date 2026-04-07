using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MercenaryListItem : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI personalityText;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider moraleSlider;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button selectButton;

    [Header("상태 색상")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color incapacitatedColor = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private Color staminaWarningColor = new Color(1f, 0.5f, 0.2f); // 60% 이하
    [SerializeField] private Color staminaCriticalColor = new Color(0.9f, 0.2f, 0.2f); // 30% 이하

    private IMercenary _mercenary;

    void Awake() {
        selectButton.onClick.AddListener(() => GameEvents.SelectMercenary(_mercenary));
    }

    public void Bind(IMercenary m) {
        _mercenary = m;
        nameText.text = m.Name;
        personalityText.text = m.PersonalityType;
        actionText.text = m.CurrentAction;

        staminaSlider.value = m.Stamina;
        moraleSlider.value = m.Morale;

        // 기력 상태에 따라 슬라이더 색상 변경
        var staminaFill = staminaSlider.fillRect.GetComponent<Image>();
        if(staminaFill != null) {
            staminaFill.color = m.Stamina switch {
                <= 30 => staminaCriticalColor,
                <= 60 => staminaWarningColor,
                _ => Color.green
            };
        }

        // 행동불능 시 배경 색상
        backgroundImage.color = m.IsIncapacitated ? incapacitatedColor : normalColor;

        // 행동불능이면 선택 버튼 비활성화
        selectButton.interactable = !m.IsIncapacitated;
    }
}