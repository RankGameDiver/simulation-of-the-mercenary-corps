using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 게임 전역의 키보드 입력을 한 곳에서 관리한다.
// 각 패널이 자기 키를 직접 처리하지 않고 여기서 바인딩을 모아 관리한다.
public class KeyboardInputManager : MonoBehaviour
{
    public static KeyboardInputManager Instance { get; private set; }

    [Serializable]
    private class PanelToggleBinding
    {
        public Key key = Key.Backquote;
        public MonoBehaviour panel; // IPanel 구현체 (Unity는 인터페이스를 직렬화하지 못함)
    }

    [Header("패널 토글 키 바인딩")]
    [SerializeField] private List<PanelToggleBinding> panelToggles = new();

    // 런타임에 확정된 매핑
    private readonly List<(Key key, IPanel panel)> _bindings = new();

    void Awake() {
        if(Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach(var binding in panelToggles) {
            if(binding.panel is IPanel panel) {
                _bindings.Add((binding.key, panel));
            } else {
                Debug.LogError($"[KeyboardInputManager] {binding.panel?.name}은(는) IPanel을 구현하지 않습니다.");
            }
        }
    }

    void Update() {
        var keyboard = Keyboard.current;
        if(keyboard == null) return; // 키보드가 없는 환경 방어

        foreach(var (key, panel) in _bindings) {
            if(keyboard[key].wasPressedThisFrame) {
                panel.Toggle();
            }
        }
    }

    // 런타임에 생성되는 패널용 등록/해제
    public void RegisterToggle(Key key, IPanel panel) => _bindings.Add((key, panel));
    public void UnregisterToggle(IPanel panel) => _bindings.RemoveAll(b => ReferenceEquals(b.panel, panel));
}
