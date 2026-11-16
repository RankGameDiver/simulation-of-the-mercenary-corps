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

    [Header("육성 화면 단축키")]
    [SerializeField] private Key[] routineKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 }; // 일과 카드 (카탈로그 순서)
    [SerializeField] private Key[] endTurnKeys = { Key.Space, Key.Enter, Key.NumpadEnter };       // 턴 넘기기
    [SerializeField] private Key growthTabKey = Key.Tab;                                            // 성장 정보 탭 전환
    [SerializeField] private Key menuKey = Key.Escape;                                              // 열린 창 닫기 / 게임 메뉴

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

        // Esc보다 먼저 확인한다: 같은 프레임에 Esc로 메뉴를 닫으면서 다른 단축키가 새어 나가지 않도록
        bool modalOpen = PanelBase.AnyModalOpen;
        if(WasPressed(keyboard, menuKey)) GameEvents.MenuRequested();

        // 메뉴·팝업이 열려 있으면 뒤쪽 게임 화면 단축키를 막는다 (마우스는 팝업의 어두운 배경이 막는다)
        if(modalOpen) return;

        foreach(var (key, panel) in _bindings) {
            if(keyboard[key].wasPressedThisFrame) {
                panel.Toggle();
            }
        }

        // 단축키는 요청 이벤트만 발행하고, 가능 여부는 받는 쪽(TurnManager, RoutinePanel 등)이 판단한다
        for(int i = 0; i < routineKeys.Length; i++) {
            if(WasPressed(keyboard, routineKeys[i])) GameEvents.RoutineHotkeyPressed(i);
        }
        foreach(var key in endTurnKeys) {
            if(WasPressed(keyboard, key)) { GameEvents.EndTurnRequested(); break; }
        }
        if(WasPressed(keyboard, growthTabKey)) GameEvents.GrowthTabCycleRequested();
    }

    private static bool WasPressed(Keyboard keyboard, Key key) =>
        key != Key.None && keyboard[key].wasPressedThisFrame;

    // 런타임에 생성되는 패널용 등록/해제
    public void RegisterToggle(Key key, IPanel panel) => _bindings.Add((key, panel));
    public void UnregisterToggle(IPanel panel) => _bindings.RemoveAll(b => ReferenceEquals(b.panel, panel));
}
