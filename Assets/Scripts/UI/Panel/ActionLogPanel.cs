using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 육성 화면 하단 로그
// - 접힘: 최근 한 줄만 표시 / 펼침: 전체 스크롤 목록
// - 토글 키(KeyboardInputManager 바인딩)는 숨김 대신 펼치기/접기로 동작한다
public class ActionLogPanel : PanelBase
{
    [Header("참조")]
    [SerializeField] private Transform logContainer;
    [SerializeField] private GameObject logLinePrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("접기/펼치기 (선택: 미연결이면 항상 펼친 목록으로 동작)")]
    [SerializeField] private RectTransform panelRect;          // 높이를 바꿀 패널 (하단 고정 앵커)
    [SerializeField] private GameObject expandedContent;       // 펼쳤을 때만 보이는 목록 영역
    [SerializeField] private TextMeshProUGUI latestLineText;   // 접혔을 때 보이는 최근 한 줄
    [SerializeField] private TextMeshProUGUI toggleHintText;   // "[`] 펼치기" / "[`] 접기"
    [SerializeField] private Button toggleButton;
    [SerializeField] private float collapsedHeight = 48f;
    [SerializeField] private float expandedHeight = 240f;
    [SerializeField] private bool expandedOnStart = false;

    [Header("설정")]
    [SerializeField] private int maxLogLines = 100;

    private readonly Queue<GameObject> _logLines = new();
    private bool _expanded;

    public bool IsExpanded => _expanded;

    protected override void Awake() {
        base.Awake();
        if(toggleButton != null) toggleButton.onClick.AddListener(Toggle);
        SetExpanded(expandedOnStart);
    }

    void OnEnable() => GameEvents.OnLogMessage += AddLog;
    void OnDisable() => GameEvents.OnLogMessage -= AddLog;

    // 토글 키: 숨기지 않고 펼치기/접기
    public override void Toggle() => SetExpanded(!_expanded);

    public void SetExpanded(bool expanded) {
        _expanded = expanded;

        if(panelRect != null) {
            var size = panelRect.sizeDelta;
            size.y = expanded ? expandedHeight : collapsedHeight;
            panelRect.sizeDelta = size;
        }
        if(expandedContent != null) expandedContent.SetActive(expanded);
        if(latestLineText != null) latestLineText.gameObject.SetActive(!expanded);
        if(toggleHintText != null) toggleHintText.text = expanded ? "[`] 접기" : "[`] 펼치기";

        if(expanded) ScrollToBottom();
    }

    private void AddLog(string message) {
        // 최대 줄 수 초과 시 가장 오래된 항목 삭제
        if(_logLines.Count >= maxLogLines) {
            var oldest = _logLines.Dequeue();
            Destroy(oldest);
        }

        var go = Instantiate(logLinePrefab, logContainer);
        go.GetComponentInChildren<TextMeshProUGUI>().text = message;
        _logLines.Enqueue(go);

        if(latestLineText != null) latestLineText.text = $"▸ {message}";

        if(_expanded || panelRect == null) ScrollToBottom();
    }

    private void ScrollToBottom() {
        if(scrollRect == null || !scrollRect.gameObject.activeInHierarchy) return;
        // Layout 반영 후 최하단으로
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Clear() {
        foreach(var line in _logLines) {
            Destroy(line);
        }

        _logLines.Clear();
        if(latestLineText != null) latestLineText.text = "";
    }
}
