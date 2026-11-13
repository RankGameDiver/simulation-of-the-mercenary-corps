using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private ActionCatalog actionCatalog; // 자율 기본 행동 주입용
    [SerializeField] private WeaponCatalog weaponCatalog; // 단일 무기 특화 롤용
    [SerializeField] private CombatBalanceConfig combatBalance; // 초기 숙련도 롤 가중치 등

    void Start() {
        // 플레이어가 관리할 용병 한 명을 생성해 TurnManager에 등록
        // (캐릭터 생성 화면이 생기면 그쪽 흐름으로 대체)
        var m = new DummyMercenaryData("카일", actionCatalog, weaponCatalog, combatBalance);
        TurnManager.Instance.SetMercenary(m);
        GameEvents.MercenaryStateChanged(m);

        string weaponInfo = m.Loadout != null
            ? $"{m.Loadout.Weapon.DisplayName}({m.Loadout.Weapon.GoverningProficiency.Korean()} Lv.{m.Loadout.WeaponLevel}, {m.Loadout.Grip.Korean()})"
            : "무기 없음";
        Debug.Log($"[용병 생성] {m.Name} (Id: {m.Id})\n" +
                  $"  힘: {m.Strength} | 민첩: {m.Agility} | 건강: {m.Health} | 지능: {m.Intelligence} | 매력: {m.Charming}\n" +
                  $"  무기: {weaponInfo}");
    }
}
