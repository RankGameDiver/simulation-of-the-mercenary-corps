using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "◀ 값 ▶" 형태의 좌우 선택 위젯. 드롭다운 대신 써서 선택지가 적은 설정을 간단히 고른다.
// 끝에서 넘기면 반대쪽 끝으로 돌아간다.
public class OptionSelector : MonoBehaviour
{
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI valueText;

    private readonly List<string> _labels = new();

    public int Index { get; private set; }
    public event Action<int> Changed; // 사용자가 값을 바꿨을 때만 (SetOptions로는 발생하지 않음)

    void Awake() {
        prevButton.onClick.AddListener(() => Step(-1));
        nextButton.onClick.AddListener(() => Step(1));
    }

    public void SetOptions(IEnumerable<string> labels, int index) {
        _labels.Clear();
        _labels.AddRange(labels);
        Index = _labels.Count == 0 ? 0 : Mathf.Clamp(index, 0, _labels.Count - 1);
        Render();
    }

    private void Step(int delta) {
        if(_labels.Count <= 1) return;
        Index = (Index + delta + _labels.Count) % _labels.Count;
        Render();
        Changed?.Invoke(Index);
    }

    private void Render() {
        valueText.text = _labels.Count > 0 ? _labels[Index] : "-";
        bool canStep = _labels.Count > 1;
        prevButton.interactable = canStep;
        nextButton.interactable = canStep;
    }
}
