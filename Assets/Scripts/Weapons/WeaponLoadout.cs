using System.Collections.Generic;

// 용병 한 명이 소지한 무기 + 운용 방식 + 숙련도 레벨 묶음.
// 런타임 데이터(디스크 직렬화 대상 아님). 단일 무기 특화 방식에서는 무기 하나의
// 지배 숙련만 레벨>0, 나머지는 0이다.
public class WeaponLoadout
{
    public WeaponDefinition Weapon { get; private set; }
    public GripMode Grip { get; private set; }

    private readonly Dictionary<Proficiency, int> _levels = new();

    public WeaponLoadout(WeaponDefinition weapon, GripMode grip) {
        Weapon = weapon;
        Grip = grip;
    }

    public IReadOnlyDictionary<Proficiency, int> Levels => _levels;

    public int LevelOf(Proficiency p) => _levels.TryGetValue(p, out var v) ? v : 0;
    public void SetLevel(Proficiency p, int level) => _levels[p] = level;

    // 장착 무기의 지배 숙련 레벨
    public int WeaponLevel => Weapon != null ? LevelOf(Weapon.GoverningProficiency) : 0;
    // 방패술 레벨 (한손+방패 운용에서 사용)
    public int ShieldLevel => LevelOf(Proficiency.ShieldArts);

    public bool IsTwoHand => Grip == GripMode.TwoHand;

    public void SetGrip(GripMode grip) => Grip = grip;
}
