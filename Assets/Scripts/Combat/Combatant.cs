using UnityEngine;

// 런타임 전투 유닛. 아군 용병(IMercenary) 또는 적(EnemyUnitDefinition)으로부터 생성되며
// 전투 중에만 존재하는 상태(HP, 포메이션 위치)를 보유한다.
public class Combatant : ICombatant
{
    public const int HpPerHealthPoint = 10; // 아군 HP = 건강 * 이 값

    public string Name { get; }
    public bool IsPlayer { get; }
    public int MaxHP { get; }
    public int CurrentHP { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    public int Strength { get; }
    public int Agility { get; }
    public WeaponLoadout Loadout { get; }

    public FormationRow Row { get; private set; }
    public int Column { get; private set; }

    public Combatant(string name, bool isPlayer, int maxHP, int strength, int agility, WeaponLoadout loadout) {
        Name = name;
        IsPlayer = isPlayer;
        MaxHP = Mathf.Max(1, maxHP);
        CurrentHP = MaxHP;
        Strength = strength;
        Agility = agility;
        Loadout = loadout;
    }

    // 아군 용병 → 전투 유닛 (HP는 건강 스탯에서 파생)
    public static Combatant FromMercenary(IMercenary m) =>
        new Combatant(m.Name, true, m.Health * HpPerHealthPoint, m.Strength, m.Agility, m.Loadout);

    // 적 정의 → 전투 유닛
    public static Combatant FromEnemy(EnemyUnitDefinition def) =>
        new Combatant(def.DisplayName, false, def.MaxHP, def.Strength, def.Agility, def.BuildLoadout());

    public void SetPosition(FormationRow row, int column) {
        Row = row;
        Column = column;
    }

    public void TakeDamage(float amount) {
        CurrentHP = Mathf.Max(0, CurrentHP - Mathf.RoundToInt(amount));
    }
}
