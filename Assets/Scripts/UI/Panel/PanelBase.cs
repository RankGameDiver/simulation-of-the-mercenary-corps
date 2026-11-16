using System.Collections.Generic;
using UnityEngine;

// CanvasGroup 기반 표시/숨김을 제공하는 UI 패널 공통 기반 클래스
// GameObject를 비활성화하지 않으므로 숨긴 동안에도 이벤트 구독과 갱신이 계속 동작한다.
[RequireComponent(typeof(CanvasGroup))]
public abstract class PanelBase : MonoBehaviour, IPanel
{
    [Header("패널 표시")]
    [SerializeField] private bool visibleOnStart = true;
    [Tooltip("모달: 열려 있는 동안 게임 단축키를 막고, Esc로 가장 나중에 연 것부터 닫힌다")]
    [SerializeField] private bool isModal;

    // 열린 모달 패널 (나중에 연 것이 뒤). 씬을 떠날 때 OnDestroy에서 빠진다.
    private static readonly List<PanelBase> OpenModals = new();

    public static bool AnyModalOpen => OpenModals.Count > 0;

    private CanvasGroup _canvasGroup;

    public bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0f;

    // 도메인 리로드를 끈 에디터 설정에서도 플레이마다 비워지도록
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => OpenModals.Clear();

    // 가장 나중에 연 모달을 닫는다. 닫을 것이 없으면 false.
    public static bool CloseTopModal() {
        if(OpenModals.Count == 0) return false;
        OpenModals[^1].Close();
        return true;
    }

    protected virtual void Awake() {
        // 씬에 이미 배치된 오브젝트에는 RequireComponent가 적용되지 않을 수 있음
        if(!TryGetComponent(out _canvasGroup)) {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SetVisible(visibleOnStart);
    }

    protected virtual void OnDestroy() => OpenModals.Remove(this);

    public void Show() => SetVisible(true);
    public void Hide() => SetVisible(false);
    // 파생 패널이 토글 키의 의미를 바꿀 수 있다 (예: 로그 패널은 숨김 대신 펼치기/접기)
    public virtual void Toggle() => SetVisible(!IsVisible);

    // Esc·닫기 요청으로 닫을 때. 닫으면서 정리할 것이 있는 패널이 재정의한다 (예: 확인 창은 콜백을 버린다).
    public virtual void Close() => Hide();

    private void SetVisible(bool visible) {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible; // 숨김 시 뒤쪽 UI 클릭이 통과하도록

        if(isModal) {
            OpenModals.Remove(this);
            if(visible) OpenModals.Add(this);
        }

        OnVisibilityChanged(visible);
    }

    // 파생 클래스에서 표시 상태 변화에 반응할 필요가 있을 때 사용
    protected virtual void OnVisibilityChanged(bool visible) { }
}
