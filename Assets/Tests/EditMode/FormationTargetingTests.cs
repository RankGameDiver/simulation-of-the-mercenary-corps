using NUnit.Framework;

// 포메이션(2열×3칸) 타겟팅·넉백 규칙 검증.
public class FormationTargetingTests
{
    // --- 후열 도달 가능 여부 ---

    [Test] public void Spear_Reaches_BackRow() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Spear(), GripMode.OneHand, 1); // 사거리 2
        Assert.IsTrue(CombatTargeting.CanReachBackRow(lo));
    }

    [Test] public void Sword_Reaches_BackRow_By_Infiltration() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.OneHand, 1); // 사거리 1이지만 후열 침투
        Assert.IsTrue(CombatTargeting.CanReachBackRow(lo));
    }

    [Test] public void Blunt_Cannot_Reach_BackRow() {
        var lo = CombatTestFactory.Loadout(CombatTestFactory.Blunt(), GripMode.OneHand, 1); // 사거리 1, 근접
        Assert.IsFalse(CombatTargeting.CanReachBackRow(lo));
    }

    // --- 주 타겟 선정 ---

    [Test] public void Spear_Can_Target_BackRow_While_Front_Alive() {
        var enemy = new Formation();
        enemy.Place(CombatTestFactory.Unit("전열병", CombatTestFactory.Sword(), GripMode.OneHandShield, 1, hp: 40), FormationRow.Front, 0);
        var backTarget = CombatTestFactory.Unit("후열궁", CombatTestFactory.Sword(), GripMode.OneHand, 1, hp: 10);
        enemy.Place(backTarget, FormationRow.Back, 0);

        var spear = CombatTestFactory.Loadout(CombatTestFactory.Spear(), GripMode.OneHand, 1);
        // 창은 후열 도달 가능 → HP 낮은 후열 대상을 고른다
        Assert.AreSame(backTarget, CombatTargeting.SelectPrimary(spear, enemy));
    }

    [Test] public void Blunt_Targets_Front_When_Back_Unreachable() {
        var enemy = new Formation();
        var frontTarget = CombatTestFactory.Unit("전열병", CombatTestFactory.Sword(), GripMode.OneHandShield, 1, hp: 40);
        enemy.Place(frontTarget, FormationRow.Front, 0);
        enemy.Place(CombatTestFactory.Unit("후열궁", CombatTestFactory.Sword(), GripMode.OneHand, 1, hp: 10), FormationRow.Back, 0);

        var blunt = CombatTestFactory.Loadout(CombatTestFactory.Blunt(), GripMode.OneHand, 1);
        // 둔기는 후열 도달 불가 → 전열만 노린다 (HP가 더 높아도)
        Assert.AreSame(frontTarget, CombatTargeting.SelectPrimary(blunt, enemy));
    }

    // --- 양손 전용 광역/관통 ---

    [Test] public void TwoHandSword_Aoe_Hits_Whole_Row() {
        var enemy = new Formation();
        // 전열 3칸을 모두 낮은 HP로 채워 주 타겟이 전열이 되도록
        enemy.Place(CombatTestFactory.Unit("전1", CombatTestFactory.Sword(), GripMode.OneHand, 1, hp: 10), FormationRow.Front, 0);
        enemy.Place(CombatTestFactory.Unit("전2", CombatTestFactory.Sword(), GripMode.OneHand, 1, hp: 12), FormationRow.Front, 1);
        enemy.Place(CombatTestFactory.Unit("전3", CombatTestFactory.Sword(), GripMode.OneHand, 1, hp: 14), FormationRow.Front, 2);
        enemy.Place(CombatTestFactory.Unit("후1", CombatTestFactory.Sword(), GripMode.OneHand, 1, hp: 40), FormationRow.Back, 0);

        var twoHandSword = CombatTestFactory.Loadout(CombatTestFactory.Sword(), GripMode.TwoHand, 3);
        var targets = CombatTargeting.SelectTargets(twoHandSword, enemy);
        Assert.AreEqual(3, targets.Count); // 전열 전체
        foreach (var t in targets) Assert.AreEqual(FormationRow.Front, t.Row);
    }

    [Test] public void TwoHandSpear_Pierce_Hits_Unit_Behind() {
        var enemy = new Formation();
        var frontTarget = CombatTestFactory.Unit("전열", CombatTestFactory.Sword(), GripMode.OneHand, 1, hp: 8);
        var behind = CombatTestFactory.Unit("후열", CombatTestFactory.Sword(), GripMode.OneHand, 1, hp: 40);
        enemy.Place(frontTarget, FormationRow.Front, 1);
        enemy.Place(behind, FormationRow.Back, 1);

        var twoHandSpear = CombatTestFactory.Loadout(CombatTestFactory.Spear(), GripMode.TwoHand, 3);
        var targets = CombatTargeting.SelectTargets(twoHandSpear, enemy);
        // 주 타겟(전열, 낮은 HP) + 바로 뒤(후열 같은 열)
        CollectionAssert.Contains(targets, frontTarget);
        CollectionAssert.Contains(targets, behind);
        Assert.AreEqual(2, targets.Count);
    }

    // --- 넉백 ---

    [Test] public void Knockback_Moves_Front_Unit_To_Back() {
        var f = new Formation();
        var unit = CombatTestFactory.Unit("피격자", CombatTestFactory.Sword(), GripMode.OneHandShield, 1);
        f.Place(unit, FormationRow.Front, 2);

        Assert.IsTrue(f.Knockback(unit));
        Assert.AreEqual(FormationRow.Back, unit.Row);
        Assert.AreEqual(2, unit.Column);
        Assert.AreSame(unit, f.At(FormationRow.Back, 2));
        Assert.IsNull(f.At(FormationRow.Front, 2));
    }

    [Test] public void Knockback_Blocked_When_Back_Occupied() {
        var f = new Formation();
        var front = CombatTestFactory.Unit("앞", CombatTestFactory.Sword(), GripMode.OneHandShield, 1);
        var back = CombatTestFactory.Unit("뒤", CombatTestFactory.Sword(), GripMode.OneHand, 1);
        f.Place(front, FormationRow.Front, 0);
        f.Place(back, FormationRow.Back, 0);

        Assert.IsFalse(f.Knockback(front)); // 뒤가 막혀 이동 불가
        Assert.AreEqual(FormationRow.Front, front.Row);
    }

    [Test] public void Place_Overflow_Spills_To_Empty_Slot() {
        var f = new Formation();
        for (int i = 0; i < Formation.Columns; i++)
            f.Place(CombatTestFactory.Unit($"전{i}", CombatTestFactory.Sword(), GripMode.OneHandShield, 1), FormationRow.Front, i);
        // 전열이 가득 찬 상태에서 전열 배치 요청 → 후열로 흘러감
        var extra = CombatTestFactory.Unit("추가", CombatTestFactory.Sword(), GripMode.OneHandShield, 1);
        Assert.IsTrue(f.Place(extra, FormationRow.Front, 1));
        Assert.AreEqual(FormationRow.Back, extra.Row);
    }
}
