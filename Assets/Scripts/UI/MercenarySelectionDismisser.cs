using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// 지정한 영역(용병 목록/상세 패널) 밖을 클릭하면 용병 선택을 해제한다.
// 빈 공간이나 다른 UI를 클릭한 경우 모두 선택 해제로 처리된다.
public class MercenarySelectionDismisser : MonoBehaviour
{
    [Header("선택 유지 영역")]
    [Tooltip("이 영역들 밖을 클릭하면 용병 선택이 해제된다. (용병 목록/상세 패널의 RectTransform)")]
    [SerializeField] private RectTransform[] keepSelectionAreas;

    // 현재 선택된 용병 (없으면 해제할 것도 없으므로 이벤트를 쏘지 않는다)
    private IMercenary _current;

    // RaycastAll 결과 재사용 버퍼
    private readonly List<RaycastResult> _raycastResults = new();

    private void OnEnable() {
        GameEvents.OnMercenarySelected += HandleMercenarySelected;
    }

    private void OnDisable() {
        GameEvents.OnMercenarySelected -= HandleMercenarySelected;
    }

    private void HandleMercenarySelected(IMercenary m) {
        _current = m;
    }

    private void Update() {
        // 선택된 용병이 없으면 해제할 대상이 없다
        if(_current == null) return;

        var mouse = Mouse.current;
        if(mouse == null) return; // 마우스가 없는 환경 방어
        if(!mouse.leftButton.wasPressedThisFrame) return;

        if(IsPointerOverKeepArea(mouse.position.ReadValue())) return;

        // 유지 영역 밖을 클릭 → 선택 해제
        GameEvents.SelectMercenary(null);
    }

    // 포인터가 선택 유지 영역(또는 그 자식) 위에 있는지 검사한다
    private bool IsPointerOverKeepArea(Vector2 screenPos) {
        if(EventSystem.current == null) return false;

        var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, _raycastResults);

        foreach(var result in _raycastResults) {
            foreach(var area in keepSelectionAreas) {
                if(area == null) continue;
                if(result.gameObject.transform.IsChildOf(area)) return true;
            }
        }

        return false;
    }
}
