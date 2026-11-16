using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 메인 화면: 새로 시작 / 이어서 시작 / 설정 / 종료
public class MainMenuController : MonoBehaviour
{
    [Header("메뉴 버튼")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("표시")]
    [SerializeField] private CanvasGroup continueGroup;        // 세이브가 없을 때 버튼 전체(글자 포함)를 흐리게
    [SerializeField] private TextMeshProUGUI continueInfoText; // "카일 · 12턴" / "저장된 진행 없음"
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("팝업")]
    [SerializeField] private SettingsPanel settingsPanel;
    [SerializeField] private ConfirmDialog confirmDialog;

    private bool _hasSave;

    void Start() {
        newGameButton.onClick.AddListener(OnNewGame);
        continueButton.onClick.AddListener(OnContinue);
        settingsButton.onClick.AddListener(settingsPanel.Show);
        quitButton.onClick.AddListener(OnQuit);

        if(versionText != null) versionText.text = $"v{Application.version}";
        RefreshContinue();
    }

    void Update() {
        // Esc: 가장 위에 열린 팝업부터 닫는다
        var keyboard = Keyboard.current;
        if(keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

        PanelBase.CloseTopModal();
    }

    // 세이브를 실제로 읽어 본 뒤 버튼을 켠다 (깨진 세이브로 이어하기를 누르지 않도록)
    private void RefreshContinue() {
        _hasSave = SaveSystem.TryLoad(out var data);
        continueButton.interactable = _hasSave;
        if(continueGroup != null) continueGroup.alpha = _hasSave ? 1f : 0.45f;
        if(continueInfoText != null) {
            continueInfoText.text = _hasSave ? $"{data.mercenary.name} · {data.turn + 1}턴" : "저장된 진행 없음";
        }
    }

    private void OnNewGame() {
        if(_hasSave) {
            confirmDialog.Open("진행 중인 용병이 있습니다.\n새로 시작하면 저장된 진행이 사라집니다.", "새로 시작", StartNewGame);
        } else {
            StartNewGame();
        }
    }

    private void StartNewGame() {
        SaveSystem.Delete();
        GameLaunch.Mode = LaunchMode.NewGame;
        SceneManager.LoadScene(SceneNames.Game);
    }

    private void OnContinue() {
        if(!_hasSave) return;
        GameLaunch.Mode = LaunchMode.Continue;
        SceneManager.LoadScene(SceneNames.Game);
    }

    private void OnQuit() => confirmDialog.Open("게임을 종료할까요?", "종료", Quit);

    private static void Quit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
