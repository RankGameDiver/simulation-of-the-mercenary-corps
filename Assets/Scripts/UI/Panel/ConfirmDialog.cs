using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 범용 확인 창: 메시지 + [취소] [확인]. 확인 버튼 문구와 동작은 열 때마다 정한다.
// (메인 화면의 종료·새로 시작 확인, 게임 메뉴의 타이틀로, 이후 출전 확인 창 등에 재사용)
public class ConfirmDialog : PanelBase
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI confirmLabel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;

    protected override void Awake() {
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Close);
        base.Awake();
    }

    public void Open(string message, string confirmText, Action onConfirm) {
        messageText.text = message;
        confirmLabel.text = confirmText;
        _onConfirm = onConfirm;
        Show();
    }

    // 취소 버튼·Esc 공통: 콜백을 버리고 닫는다
    public override void Close() {
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
