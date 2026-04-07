using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ActionLogPanel : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform logContainer;
    [SerializeField] private GameObject logLinePrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("설정")]
    [SerializeField] private int maxLogLines = 100;

    private readonly Queue<GameObject> _logLines = new();

    void OnEnable() => GameEvents.OnLogMessage += AddLog;
    void OnDisable() => GameEvents.OnLogMessage -= AddLog;

    private void AddLog(string message) {
        // 최대 줄 수 초과 시 가장 오래된 항목 삭제
        if(_logLines.Count >= maxLogLines) {
            var oldest = _logLines.Dequeue();
            Destroy(oldest);
        }

        var go = Instantiate(logLinePrefab, logContainer);
        go.GetComponentInChildren<TextMeshProUGUI>().text = message;
        _logLines.Enqueue(go);

        // 스크롤 최하단으로 (한 프레임 뒤에 실행해야 Layout 반영됨)
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Clear() {
        foreach(var line in _logLines) {
            Destroy(line);
        }
        
        _logLines.Clear();
    }
}