using UnityEngine;

// 전투 수식 (순수 규칙, UnityEngine.Mathf 외 의존 없음).
// 유효 공격력·명중·회피·행동속도·기력·피해 감소·최종 피해를 계산한다.
public static class CombatMath
{
    // 유효 공격력 = (힘*힘가중 + 민첩*민첩가중) * (1 + 숙련 피해보정)
    // 검=민첩, 창=(힘+민첩)/2, 둔기=힘 스케일을 무기의 가중치로 표현.
    public static float EffectiveAttack(int strength, int agility, WeaponLoadout loadout, ProficiencyCurve curve) {
        if (loadout?.Weapon == null || curve == null) return 0f;
        var w = loadout.Weapon;
        float statBase = strength * w.StrengthWeight + agility * w.AgilityWeight;
        return statBase * (1f + curve.DamageBonus(loadout.WeaponLevel));
    }

    // 명중 보정 = 숙련 명중 + 운용 명중(오프핸드 +10%)
    public static float HitModifier(WeaponLoadout loadout, ProficiencyCurve curve, CombatBalanceConfig cfg) {
        float hit = curve.HitBonus(loadout.WeaponLevel);
        if (loadout.Grip == GripMode.OneHand) hit += cfg.OffhandHitBonus;
        return hit;
    }

    // 회피 보정 = 오프핸드 +10%
    public static float EvasionModifier(WeaponLoadout loadout, CombatBalanceConfig cfg) {
        return loadout.Grip == GripMode.OneHand ? cfg.OffhandEvasionBonus : 0f;
    }

    // 행동속도 배수 (1.0 기준): 오프핸드 +10%, 방패 -10%, 양손 -20%
    public static float SpeedMultiplier(WeaponLoadout loadout, CombatBalanceConfig cfg) {
        return loadout.Grip switch {
            GripMode.OneHand => 1f + cfg.OffhandSpeedBonus,
            GripMode.OneHandShield => 1f + cfg.ShieldSpeedPenalty,
            GripMode.TwoHand => 1f + cfg.TwoHandSpeedPenalty,
            _ => 1f,
        };
    }

    // 기력 소모 배수: 오프핸드 -20%, 양손 x1.5
    public static float StaminaMultiplier(WeaponLoadout loadout, CombatBalanceConfig cfg) {
        return loadout.Grip switch {
            GripMode.OneHand => 1f - cfg.OffhandStaminaReduction,
            GripMode.TwoHand => cfg.TwoHandStaminaMultiplier,
            _ => 1f,
        };
    }

    // 방어측 피해 감소율 (한손+방패 운용에서만, 방패술 숙련도에 비례)
    public static float DamageReduction(WeaponLoadout defenderLoadout, ProficiencyCurve curve, CombatBalanceConfig cfg) {
        if (defenderLoadout == null || defenderLoadout.Grip != GripMode.OneHandShield) return 0f;
        return cfg.ShieldDamageReduction(defenderLoadout.ShieldLevel, curve.MaxLevel);
    }

    // 최종 피해 = 유효공격 * 상성배율 * (1 - 피해감소).
    // ignoreDefense(양손둔기 등 방어 무시)면 피해 감소를 무시한다.
    public static float ResolveDamage(
        float effectiveAttack,
        MatchupResult matchup,
        bool attackerTwoHand,
        bool ignoreDefense,
        float defenderDamageReduction,
        CombatBalanceConfig cfg) {
        float dmg = effectiveAttack * WeaponMatchup.Multiplier(matchup, attackerTwoHand, cfg);
        if (!ignoreDefense) dmg *= 1f - defenderDamageReduction;
        return Mathf.Max(0f, dmg);
    }
}
