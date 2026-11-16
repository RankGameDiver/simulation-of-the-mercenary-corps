using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 게임 메뉴 (UI_육성화면 3.1의 [≡]): 계속하기 / 설정 / 타이틀로
// 상단 바 메뉴 버튼이나 Esc(GameEvents.MenuRequested)로 연다.
public class GameMenuPanel : PanelBase
{
    [Header("열기")]
    [SerializeField] private Button openButton; // 상단 바 [≡]

    [Header("메뉴 버튼")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button titleButton;

    [Header("팝업")]
    [SerializeField] private SettingsPanel settingsPanel;
    [SerializeField] private ConfirmDialog confirmDialog;

    protected override void Awake() {
        openButton.onClick.AddListener(Show);
        resumeButton.onClick.AddListener(Hide);
        settingsButton.onClick.AddListener(settingsPanel.Show);
        titleButton.onClick.AddListener(OnTitle);
        base.Awake();
    }

    void OnEnable() => GameEvents.OnMenuRequested += OnMenuRequested;
    void OnDisable() => GameEvents.OnMenuRequested -= OnMenuRequested;

    // Esc: 열린 창이 있으면 가장 위의 것을 닫고, 없으면 메뉴를 연다
    private void OnMenuRequested() {
        if(!CloseTopModal()) Show();
    }

    // 진행은 매 턴 계획 단계 시작 시 자동 저장되므로 여기서 따로 저장하지 않는다 (기획서 11.2)
    private void OnTitle() =>
        confirmDialog.Open("타이틀로 돌아갈까요?\n진행은 이번 턴 시작 시점으로 저장되어 있습니다.", "타이틀로",
            () => SceneManager.LoadScene(SceneNames.MainMenu));
}
