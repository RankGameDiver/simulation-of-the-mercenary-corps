using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private int dummyMercenaryCount = 5;
    [SerializeField] private ActionCatalog actionCatalog; // 자율 기본 행동 주입용
    [SerializeField] private WeaponCatalog weaponCatalog; // 단일 무기 특화 롤용
    [SerializeField] private CombatBalanceConfig combatBalance; // 초기 숙련도 롤 가중치 등

    void Start() {
        // 더미 용병 생성 후 TurnManager에 등록
        var names = new[] { "카일", "레나", "미아", "루드거" };
        for (int i = 0; i < Mathf.Min(dummyMercenaryCount, names.Length); i++) {
            var m = new DummyMercenaryData(names[i], actionCatalog, weaponCatalog, combatBalance);
            TurnManager.Instance.RegisterMercenary(m);

            string weaponInfo = m.Loadout != null
                ? $"{m.Loadout.Weapon.DisplayName}({m.Loadout.Weapon.GoverningProficiency.Korean()} Lv.{m.Loadout.WeaponLevel}, {m.Loadout.Grip.Korean()})"
                : "무기 없음";
            Debug.Log($"[용병 생성] {m.Name} (Id: {m.Id})\n" +
                      $"  힘: {m.Strength} | 민첩: {m.Agility} | 건강: {m.Health} | 지능: {m.Intelligence} | 매력: {m.Charming}\n" +
                      $"  무기: {weaponInfo}");
        }
    }
}