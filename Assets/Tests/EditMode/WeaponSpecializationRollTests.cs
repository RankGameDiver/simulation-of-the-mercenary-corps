using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// 초기 숙련도 롤(가중 Lv.1~5)과 단일 무기 특화 생성 검증.
public class WeaponSpecializationRollTests
{
    private static readonly int[] Weights = { 40, 25, 18, 12, 5 };

    [Test] public void WeightedLevel_Stays_In_Range() {
        Random.InitState(1234);
        for (int i = 0; i < 500; i++) {
            int lv = WeaponRoll.WeightedLevel(Weights);
            Assert.That(lv, Is.InRange(1, 5));
        }
    }

    [Test] public void HigherLevels_Are_Rarer() {
        Random.InitState(4321);
        var counts = new Dictionary<int, int> { {1,0},{2,0},{3,0},{4,0},{5,0} };
        for (int i = 0; i < 5000; i++) counts[WeaponRoll.WeightedLevel(Weights)]++;

        // 가중치 40 > 5 이므로 Lv.1이 Lv.5보다 확연히 흔해야 한다.
        Assert.Greater(counts[1], counts[5]);
        Assert.Greater(counts[2], counts[5]);
    }

    [Test] public void EmptyWeights_Fallback_To_Level1() {
        Assert.AreEqual(1, WeaponRoll.WeightedLevel(null));
        Assert.AreEqual(1, WeaponRoll.WeightedLevel(new int[0]));
    }

    [Test] public void Mercenary_Specializes_In_Single_Weapon() {
        var weapons = CombatTestFactory.WeaponCatalog(
            CombatTestFactory.Sword(), CombatTestFactory.Spear(), CombatTestFactory.Blunt());
        var merc = new DummyMercenaryData(
            "테스트", CombatTestFactory.EmptyActionCatalog(), weapons, CombatTestFactory.Config());

        Assert.IsNotNull(merc.Loadout);
        var governing = merc.Loadout.Weapon.GoverningProficiency;
        Assert.That(merc.Loadout.WeaponLevel, Is.InRange(1, 5));

        // 지배 숙련만 레벨>0, 나머지 숙련은 0
        foreach (Proficiency p in System.Enum.GetValues(typeof(Proficiency))) {
            if (p == governing) continue;
            Assert.AreEqual(0, merc.Loadout.LevelOf(p), $"{p} 숙련은 0이어야 함");
        }
    }

    [Test] public void No_WeaponCatalog_Leaves_Loadout_Null() {
        var merc = new DummyMercenaryData("무장없음", CombatTestFactory.EmptyActionCatalog());
        Assert.IsNull(merc.Loadout);
    }
}
