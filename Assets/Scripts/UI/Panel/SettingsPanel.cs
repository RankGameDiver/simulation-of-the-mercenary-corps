using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 설정 창 (UI_육성화면 3.6). 바꾼 값은 바로 적용되고 GameSettings(PlayerPrefs)에 저장된다.
public class SettingsPanel : PanelBase
{
    [Header("옵션")]
    [SerializeField] private OptionSelector screenModeSelector;
    [SerializeField] private OptionSelector resolutionSelector;
    [SerializeField] private OptionSelector skipResultPopupSelector;

    [SerializeField] private Button closeButton;

    private static readonly string[] ScreenModeLabels = { "창 모드", "전체 화면" }; // 인덱스 1 = 전체 화면
    private static readonly string[] OnOffLabels = { "끄기", "켜기" };              // 인덱스 1 = 켜기

    private List<Vector2Int> _resolutions = new();

    protected override void Awake() {
        closeButton.onClick.AddListener(Hide);
        screenModeSelector.Changed += i => GameSettings.FullScreen = i == 1;
        resolutionSelector.Changed += i => GameSettings.Resolution = _resolutions[i];
        skipResultPopupSelector.Changed += i => GameSettings.SkipTurnResultPopup = i == 1;
        base.Awake();
    }

    // 열 때마다 현재 값으로 다시 채운다 (해상도 목록은 모니터에 따라 바뀔 수 있다)
    protected override void OnVisibilityChanged(bool visible) {
        if(visible) Refresh();
    }

    private void Refresh() {
        screenModeSelector.SetOptions(ScreenModeLabels, GameSettings.FullScreen ? 1 : 0);

        _resolutions = GameSettings.AvailableResolutions();
        var labels = new List<string>(_resolutions.Count);
        foreach(var r in _resolutions) labels.Add($"{r.x} × {r.y}");
        resolutionSelector.SetOptions(labels, _resolutions.IndexOf(GameSettings.Resolution));

        skipResultPopupSelector.SetOptions(OnOffLabels, GameSettings.SkipTurnResultPopup ? 1 : 0);
    }
}
