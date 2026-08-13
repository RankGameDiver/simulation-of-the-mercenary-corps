using UnityEngine;

// 전투 프로토타입을 손쉽게 시작하기 위한 디버그 진입점.
// UI 버튼 onClick에 StartCombat()을 연결하거나, 플레이 모드에서 컴포넌트
// 우클릭 컨텍스트 메뉴 "전투 시작"으로 호출한다. 진행은 ActionLogPanel(GameEvents.Log)로 관찰.
public class CombatDebugStarter : MonoBehaviour
{
    [SerializeField] private EncounterDefinition encounter; // 시작할 조우

    [ContextMenu("전투 시작")]
    public void StartCombat() {
        if (CombatManager.Instance == null) {
            Debug.LogWarning("[전투 디버그] 씬에 CombatManager가 없습니다");
            return;
        }
        if (encounter == null) {
            Debug.LogWarning("[전투 디버그] 조우(EncounterDefinition)가 지정되지 않았습니다");
            return;
        }

        var party = TurnManager.Instance != null ? TurnManager.Instance.GetMercenaries() : null;
        CombatManager.Instance.StartEncounter(party, encounter);
    }
}
