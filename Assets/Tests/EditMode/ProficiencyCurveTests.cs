using NUnit.Framework;

// 숙련도 곡선(Lv1~5)과 해금 게이트(Lv3 양손, Lv5 필살기) 검증.
public class ProficiencyCurveTests
{
    private ProficiencyCurve _curve;

    [SetUp] public void SetUp() => _curve = CombatTestFactory.Curve();

    [Test] public void Level0_Has_No_Bonus() {
        Assert.That(_curve.HitBonus(0), Is.EqualTo(0f).Within(1e-4f));
        Assert.That(_curve.DamageBonus(0), Is.EqualTo(0f).Within(1e-4f));
    }

    [Test] public void Level1_Matches_Doc() {
        Assert.That(_curve.HitBonus(1), Is.EqualTo(0.03f).Within(1e-4f));
        Assert.That(_curve.DamageBonus(1), Is.EqualTo(0.05f).Within(1e-4f));
    }

    [Test] public void Level3_Matches_Doc() {
        Assert.That(_curve.HitBonus(3), Is.EqualTo(0.09f).Within(1e-4f));
        Assert.That(_curve.DamageBonus(3), Is.EqualTo(0.15f).Within(1e-4f));
    }

    [Test] public void Level5_Matches_Doc() {
        Assert.That(_curve.HitBonus(5), Is.EqualTo(0.15f).Within(1e-4f));
        Assert.That(_curve.DamageBonus(5), Is.EqualTo(0.25f).Within(1e-4f));
    }

    [Test] public void TwoHand_Unlocks_At_Level3() {
        Assert.IsFalse(_curve.IsTwoHandUnlocked(2));
        Assert.IsTrue(_curve.IsTwoHandUnlocked(3));
    }

    [Test] public void Ultimate_Unlocks_At_Level5() {
        Assert.IsFalse(_curve.IsUltimateUnlocked(4));
        Assert.IsTrue(_curve.IsUltimateUnlocked(5));
    }

    [Test] public void MaxLevel_Is_5() => Assert.AreEqual(5, _curve.MaxLevel);

    [Test] public void Level_Above_Max_Clamps() =>
        Assert.That(_curve.DamageBonus(99), Is.EqualTo(0.25f).Within(1e-4f));
}
