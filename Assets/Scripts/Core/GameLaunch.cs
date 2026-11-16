// 메인 화면에서 Game 씬으로 넘어갈 때 시작 방식을 전달한다.
// Game 씬을 에디터에서 바로 실행하면 기본값(새 게임)으로 시작한다.
public enum LaunchMode { NewGame, Continue }

public static class GameLaunch
{
    public static LaunchMode Mode { get; set; } = LaunchMode.NewGame;

    // 한 번 읽으면 기본값으로 되돌린다 (다음 진입에 이전 선택이 남지 않도록)
    public static LaunchMode Consume() {
        var mode = Mode;
        Mode = LaunchMode.NewGame;
        return mode;
    }
}
