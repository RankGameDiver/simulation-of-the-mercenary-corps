using NUnit.Framework;

// 유효 공격 스케일링(무기별)과 운용 보정(오프핸드/방패/양손), 최종 피해 계산 검증.
public class CombatMathTests
{
    private ProficiencyCurve _curve;
    private CombatBalanceConfig _cfg;

    [SetUp] public void SetUp() {
        _curve = CombatTestFactory.Curve();
        _cfg = CombatTestFactory.Config();
    }

    [Test] public void Sword_Scales_With_Agility() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.OneHandShield, weaponLevel: 0);
        // 검=(힘0, 민첩1): 힘3 민첩8, 레벨0 → 8
        Assert.That(CombatMath.EffectiveAttack(3, 8, lo, _curve), Is.EqualTo(8f).Within(1e-4f));
    }

    [Test] public void Spear_Scales_With_Str_And_Agi_Average() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Spear(), GripMode.OneHandShield, weaponLevel: 0);
        // 창=(0.5,0.5): 힘6 민첩4 → 5
        Assert.That(CombatMath.EffectiveAttack(6, 4, lo, _curve), Is.EqualTo(5f).Within(1e-4f));
    }

    [Test] public void Blunt_Scales_With_Strength() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Blunt(), GripMode.OneHandShield, weaponLevel: 0);
        // 둔기=(1,0): 힘9 → 9
        Assert.That(CombatMath.EffectiveAttack(9, 3, lo, _curve), Is.EqualTo(9f).Within(1e-4f));
    }

    [Test] public void ProficiencyLevel_Boosts_Damage() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.OneHandShield, weaponLevel: 1);
        // 민첩8, 레벨1 피해보정 +5% → 8*1.05 = 8.4
        Assert.That(CombatMath.EffectiveAttack(3, 8, lo, _curve), Is.EqualTo(8.4f).Within(1e-4f));
    }

    [Test] public void SpeedMultiplier_By_Grip() {
        Assert.That(CombatMath.SpeedMultiplier(OneHand(), _cfg), Is.EqualTo(1.1f).Within(1e-4f));
        Assert.That(CombatMath.SpeedMultiplier(Shield(), _cfg), Is.EqualTo(0.9f).Within(1e-4f));
        Assert.That(CombatMath.SpeedMultiplier(TwoHand(), _cfg), Is.EqualTo(0.8f).Within(1e-4f));
    }

    [Test] public void StaminaMultiplier_By_Grip() {
        Assert.That(CombatMath.StaminaMultiplier(OneHand(), _cfg), Is.EqualTo(0.8f).Within(1e-4f));
        Assert.That(CombatMath.StaminaMultiplier(Shield(), _cfg), Is.EqualTo(1f).Within(1e-4f));
        Assert.That(CombatMath.StaminaMultiplier(TwoHand(), _cfg), Is.EqualTo(1.5f).Within(1e-4f));
    }

    [Test] public void Offhand_Adds_Hit_Bonus() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.OneHand, weaponLevel: 1);
        // 숙련 명중 +3% + 오프핸드 +10% = 13%
        Assert.That(CombatMath.HitModifier(lo, _curve, _cfg), Is.EqualTo(0.13f).Within(1e-4f));
    }

    [Test] public void Shield_DamageReduction_Scales_With_ShieldLevel() {
        var noSkill = CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.OneHandShield, 1, shieldLevel: 0);
        var maxSkill = CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.OneHandShield, 1, shieldLevel: 5);
        Assert.That(CombatMath.DamageReduction(noSkill, _curve, _cfg), Is.EqualTo(0.15f).Within(1e-4f));
        Assert.That(CombatMath.DamageReduction(maxSkill, _curve, _cfg), Is.EqualTo(0.25f).Within(1e-4f));
    }

    [Test] public void NonShieldGrip_Has_No_DamageReduction() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.TwoHand, 3);
        Assert.That(CombatMath.DamageReduction(lo, _curve, _cfg), Is.EqualTo(0f).Within(1e-4f));
    }

    [Test] public void ResolveDamage_Applies_Matchup_And_Reduction() {
        // 유효공격 10, 유리(한손) x1.2, 감소 20% → 10*1.2*0.8 = 9.6
        float dmg = CombatMath.ResolveDamage(10f, MatchupResult.Advantage, false, false, 0.2f, _cfg);
        Assert.That(dmg, Is.EqualTo(9.6f).Within(1e-4f));
    }

    [Test] public void ResolveDamage_IgnoreDefense_Skips_Reduction() {
        // 방어 무시 → 감소 미적용: 10*1.2 = 12
        float dmg = CombatMath.ResolveDamage(10f, MatchupResult.Advantage, false, true, 0.2f, _cfg);
        Assert.That(dmg, Is.EqualTo(12f).Within(1e-4f));
    }

    private WeaponLoadout OneHand() => CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.OneHand, 1);
    private WeaponLoadout Shield() => CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.OneHandShield, 1);
    private WeaponLoadout TwoHand() => CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.TwoHand, 3);
}
