using UnityEngine;

// Game 씬 진입점. 메인 화면에서 고른 방식(GameLaunch)에 따라 용병을 새로 만들거나 세이브에서 되살리고,
// 계획 단계가 시작될 때마다 진행 상황을 자동 저장한다.
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private ActionCatalog actionCatalog; // 해금 롤·세이브 복원용
    [SerializeField] private WeaponCatalog weaponCatalog; // 단일 무기 특화 롤·세이브 복원용
    [SerializeField] private CombatBalanceConfig combatBalance; // 초기 숙련도 롤 가중치 등

    private MercenaryData _mercenary;

    void OnEnable() => GameEvents.OnPlanningPhaseStart += AutoSave;
    void OnDisable() => GameEvents.OnPlanningPhaseStart -= AutoSave;

    void Start() {
        if(GameLaunch.Consume() == LaunchMode.Continue && TryContinue()) return;
        StartNewGame();
    }

    private void StartNewGame() {
        // 플레이어가 관리할 용병 한 명을 생성한다 (캐릭터 생성 화면이 생기면 그쪽 흐름으로 대체)
        var m = new DummyMercenaryData("카일", actionCatalog, weaponCatalog, combatBalance);
        _mercenary = m;
        LogMercenary("용병 생성", m);
        TurnManager.Instance.BeginGame(m); // 계획 단계 시작 → 첫 자동 저장
    }

    private bool TryContinue() {
        if(!SaveSystem.TryLoad(out var data)) {
            Debug.LogWarning("[이어하기] 불러올 세이브가 없어 새로 시작합니다.");
            return false;
        }

        var m = DummyMercenaryData.FromSave(data.mercenary, actionCatalog, weaponCatalog);
        _mercenary = m;
        LogMercenary("이어하기", m);
        TurnManager.Instance.BeginGame(m, data.turn);
        GameEvents.Log($"저장된 진행을 불러왔습니다 ({data.turn + 1}턴)");
        return true;
    }

    // 계획 단계에서만 저장한다. 실행 중에는 일과 효과가 반쯤 적용된 상태일 수 있다.
    private void AutoSave() {
        if(_mercenary == null || TurnManager.Instance == null) return;
        SaveSystem.Save(new SaveData {
            turn = TurnManager.Instance.CurrentTurn,
            mercenary = _mercenary.ToSaveData(),
        });
    }

    private static void LogMercenary(string tag, MercenaryData m) {
        string weaponInfo = m.Loadout != null
            ? $"{m.Loadout.Weapon.DisplayName}({m.Loadout.Weapon.GoverningProficiency.Korean()} Lv.{m.Loadout.WeaponLevel}, {m.Loadout.Grip.Korean()})"
            : "무기 없음";
        Debug.Log($"[{tag}] {m.Name} (Id: {m.Id})\n" +
                  $"  힘: {m.Strength} | 민첩: {m.Agility} | 건강: {m.Health} | 지능: {m.Intelligence} | 매력: {m.Charming}\n" +
                  $"  무기: {weaponInfo}");
    }
}
