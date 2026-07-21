// 키보드 등 외부 시스템에서 표시 상태를 조작할 수 있는 UI 패널
public interface IPanel
{
    bool IsVisible { get; }

    void Show();
    void Hide();
    void Toggle();
}
