using System.Reflection;
using UnityEngine;

// 테스트용 팩토리. ScriptableObject의 private [SerializeField] 필드를 리플렉션으로 채워
// 특정 무기/설정 인스턴스를 만든다. (에디터에서 애셋을 만들지 않고 순수 규칙을 검증)
public static class CombatTestFactory
{
    public static void SetPrivate(object obj, string field, object value) {
        var f = obj.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null) throw new System.ArgumentException($"필드를 찾을 수 없음: {field} on {obj.GetType().Name}");
        f.SetValue(obj, value);
    }

    public static WeaponDefinition Weapon(
        WeaponClass cls, Proficiency prof, float strengthWeight, float agilityWeight,
        int oneHandRange = 1, int twoHandRange = 1, TwoHandEffect effect = TwoHandEffect.None) {
        var w = ScriptableObject.CreateInstance<WeaponDefinition>();
        SetPrivate(w, "weaponClass", cls);
        SetPrivate(w, "governingProficiency", prof);
        SetPrivate(w, "strengthWeight", strengthWeight);
        SetPrivate(w, "agilityWeight", agilityWeight);
        SetPrivate(w, "oneHandRange", oneHandRange);
        SetPrivate(w, "twoHandRange", twoHandRange);
        SetPrivate(w, "twoHandEffect", effect);
        return w;
    }

    // 문서 기준 표준 무기 3종
    public static WeaponDefinition Sword() => Weapon(WeaponClass.Sword, Proficiency.Swordsmanship, 0f, 1f, 1, 1, TwoHandEffect.AoeRow);
    public static WeaponDefinition Spear() => Weapon(WeaponClass.Spear, Proficiency.Spearmanship, 0.5f, 0.5f, 2, 3, TwoHandEffect.Pierce);
    public static WeaponDefinition Blunt() => Weapon(WeaponClass.Blunt, Proficiency.BluntArts, 1f, 0f, 1, 1, TwoHandEffect.ArmorBreakKnockback);

    public static ProficiencyCurve Curve() => ScriptableObject.CreateInstance<ProficiencyCurve>();
    public static CombatBalanceConfig Config() => ScriptableObject.CreateInstance<CombatBalanceConfig>();

    public static WeaponCatalog WeaponCatalog(params WeaponDefinition[] weapons) {
        var c = ScriptableObject.CreateInstance<WeaponCatalog>();
        SetPrivate(c, "weapons", weapons);
        return c;
    }

    // 무기 롤에 영향을 주지 않는 최소 행동 카탈로그 (actions 비어 있음, default null)
    public static ActionCatalog EmptyActionCatalog() {
        var c = ScriptableObject.CreateInstance<ActionCatalog>();
        SetPrivate(c, "actions", new ActionDefinition[0]);
        return c;
    }

    public static WeaponLoadout Loadout(WeaponDefinition weapon, GripMode grip, int weaponLevel, int shieldLevel = 0) {
        var lo = new WeaponLoadout(weapon, grip);
        lo.SetLevel(weapon.GoverningProficiency, weaponLevel);
        if (shieldLevel > 0) lo.SetLevel(Proficiency.ShieldArts, shieldLevel);
        return lo;
    }

    // 테스트용 전투 유닛. weapon이 null이면 맨손.
    public static Combatant Unit(string name, WeaponDefinition weapon, GripMode grip,
                                 int weaponLevel, int hp = 40, int str = 5, int agi = 5) {
        var lo = weapon != null ? Loadout(weapon, grip, weaponLevel) : null;
        return new Combatant(name, false, hp, str, agi, lo);
    }
}
