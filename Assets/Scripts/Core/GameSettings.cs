using System.Collections.Generic;
using UnityEngine;

// 기기별 설정 (PlayerPrefs). 세이브 데이터와 별개라 용병이 죽고 새로 시작해도 유지된다 (UI_육성화면 3.6 [제안]).
// 메인 화면 설정 창과 육성 화면 [≡] 설정 창이 함께 쓴다.
public static class GameSettings
{
    private const string SkipTurnResultPopupKey = "settings.skipTurnResultPopup";
    private const string FullScreenKey = "settings.fullScreen";
    private const string ResolutionWidthKey = "settings.resolutionWidth";
    private const string ResolutionHeightKey = "settings.resolutionHeight";

    // 턴 결과 팝업 생략 (기본 꺼짐, [확정]). 결과 팝업을 구현할 때 이 값을 읽는다.
    public static bool SkipTurnResultPopup {
        get => PlayerPrefs.GetInt(SkipTurnResultPopupKey, 0) == 1;
        set {
            PlayerPrefs.SetInt(SkipTurnResultPopupKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    // 전체 화면(테두리 없는 창 모드) / 창 모드. 저장값이 없으면 지금 화면 상태를 따른다.
    public static bool FullScreen {
        get => PlayerPrefs.GetInt(FullScreenKey, Screen.fullScreen ? 1 : 0) == 1;
        set {
            PlayerPrefs.SetInt(FullScreenKey, value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyDisplay();
        }
    }

    // 해상도 (너비·높이만, 주사율은 다루지 않는다). 저장값이 없으면 지금 화면 크기를 따른다.
    public static Vector2Int Resolution {
        get => new(PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width),
                   PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height));
        set {
            PlayerPrefs.SetInt(ResolutionWidthKey, value.x);
            PlayerPrefs.SetInt(ResolutionHeightKey, value.y);
            PlayerPrefs.Save();
            ApplyDisplay();
        }
    }

    // 모니터가 지원하는 해상도 목록 (중복 제거, 작은 것부터). 지금 해상도가 목록에 없으면 끼워 넣는다.
    public static List<Vector2Int> AvailableResolutions() {
        var set = new SortedSet<Vector2Int>(Comparer<Vector2Int>.Create((a, b) =>
            a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y)));
        foreach(var r in Screen.resolutions) set.Add(new Vector2Int(r.width, r.height));
        set.Add(Resolution);
        return new List<Vector2Int>(set);
    }

    public static void ApplyDisplay() {
        var res = Resolution;
        var mode = FullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(res.x, res.y, mode);
    }

    // 실행 시 저장된 화면 설정을 적용한다 (에디터의 Game 뷰에는 적용하지 않는다)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyOnLaunch() {
        if(Application.isEditor) return;
        if(PlayerPrefs.HasKey(FullScreenKey) || PlayerPrefs.HasKey(ResolutionWidthKey)) ApplyDisplay();
    }
}
