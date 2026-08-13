using NUnit.Framework;

// 상성 고리(창>검>둔기>창)와 배율(±20%, 양손 ±30%) 검증.
public class WeaponMatchupTests
{
    private CombatBalanceConfig _cfg;

    [SetUp] public void SetUp() => _cfg = CombatTestFactory.Config();

    [Test] public void Spear_Beats_Sword() =>
        Assert.AreEqual(MatchupResult.Advantage, WeaponMatchup.Evaluate(WeaponClass.Spear, WeaponClass.Sword, _cfg));

    [Test] public void Sword_Beats_Blunt() =>
        Assert.AreEqual(MatchupResult.Advantage, WeaponMatchup.Evaluate(WeaponClass.Sword, WeaponClass.Blunt, _cfg));

    [Test] public void Blunt_Beats_Spear() =>
        Assert.AreEqual(MatchupResult.Advantage, WeaponMatchup.Evaluate(WeaponClass.Blunt, WeaponClass.Spear, _cfg));

    [Test] public void Sword_Loses_To_Spear() =>
        Assert.AreEqual(MatchupResult.Disadvantage, WeaponMatchup.Evaluate(WeaponClass.Sword, WeaponClass.Spear, _cfg));

    [Test] public void SameClass_Is_Neutral() =>
        Assert.AreEqual(MatchupResult.Neutral, WeaponMatchup.Evaluate(WeaponClass.Sword, WeaponClass.Sword, _cfg));

    [Test] public void OneHand_Advantage_Is_Plus20() =>
        Assert.That(WeaponMatchup.Multiplier(MatchupResult.Advantage, false, _cfg), Is.EqualTo(1.2f).Within(1e-4f));

    [Test] public void OneHand_Disadvantage_Is_Minus20() =>
        Assert.That(WeaponMatchup.Multiplier(MatchupResult.Disadvantage, false, _cfg), Is.EqualTo(0.8f).Within(1e-4f));

    [Test] public void TwoHand_Advantage_Is_Plus30() =>
        Assert.That(WeaponMatchup.Multiplier(MatchupResult.Advantage, true, _cfg), Is.EqualTo(1.3f).Within(1e-4f));

    [Test] public void TwoHand_Disadvantage_Is_Minus30() =>
        Assert.That(WeaponMatchup.Multiplier(MatchupResult.Disadvantage, true, _cfg), Is.EqualTo(0.7f).Within(1e-4f));

    [Test] public void TwoHand_Neutral_Is_Unchanged() =>
        Assert.That(WeaponMatchup.Multiplier(MatchupResult.Neutral, true, _cfg), Is.EqualTo(1f).Within(1e-4f));
}
