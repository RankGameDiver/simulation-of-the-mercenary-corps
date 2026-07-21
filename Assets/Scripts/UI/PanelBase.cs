using UnityEngine;

// CanvasGroup 기반 표시/숨김을 제공하는 UI 패널 공통 기반 클래스
// GameObject를 비활성화하지 않으므로 숨긴 동안에도 이벤트 구독과 갱신이 계속 동작한다.
[RequireComponent(typeof(CanvasGroup))]
public abstract class PanelBase : MonoBehaviour, IPanel
{
    [Header("패널 표시")]
    [SerializeField] private bool visibleOnStart = true;

    private CanvasGroup _canvasGroup;

    public bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0f;

    protected virtual void Awake() {
        // 씬에 이미 배치된 오브젝트에는 RequireComponent가 적용되지 않을 수 있음
        if(!TryGetComponent(out _canvasGroup)) {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SetVisible(visibleOnStart);
    }

    public void Show() => SetVisible(true);
    public void Hide() => SetVisible(false);
    public void Toggle() => SetVisible(!IsVisible);

    private void SetVisible(bool visible) {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible; // 숨김 시 뒤쪽 UI 클릭이 통과하도록

        OnVisibilityChanged(visible);
    }

    // 파생 클래스에서 표시 상태 변화에 반응할 필요가 있을 때 사용
    protected virtual void OnVisibilityChanged(bool visible) { }
}
