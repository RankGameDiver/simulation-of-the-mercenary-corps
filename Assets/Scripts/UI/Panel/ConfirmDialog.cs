using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 범용 확인 창: 메시지 + [취소] [확인]. 확인 버튼 문구와 동작은 열 때마다 정한다.
// (메인 화면의 종료·새로 시작 확인, 이후 출전 확인 창 등에 재사용)
public class ConfirmDialog : PanelBase
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI confirmLabel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;

    protected override void Awake() {
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);
        base.Awake();
    }

    public void Open(string message, string confirmText, Action onConfirm) {
        messageText.text = message;
        confirmLabel.text = confirmText;
        _onConfirm = onConfirm;
        Show();
    }

    public void Cancel() {
        _onConfirm = null;
        Hide();
    }

    private void Confirm() {
        var callback = _onConfirm;
        _onConfirm = null;
        Hide();
        callback?.Invoke();
    }
}
