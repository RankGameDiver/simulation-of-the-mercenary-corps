using System.Collections.Generic;

// 사거리·무기 특성에 따른 타겟팅 규칙 (순수 규칙, 테스트 대상).
// - 사거리 2 이상(창): 후열 직접 타격
// - 검: 기동/후열 침투로 후열 도달
// - 그 외 근접(둔기 등): 전열이 살아 있으면 후열 불가
// - 양손검 광역(AoeRow): 대상의 열 전체
// - 양손창 관통(Pierce): 대상 + 바로 뒤 유닛(직선 2체)
public static class CombatTargeting
{
    public static bool CanReachBackRow(WeaponLoadout lo) {
        if (lo?.Weapon == null) return false;
        if (lo.Weapon.RangeFor(lo.Grip) >= 2) return true;   // 창(사거리) / 양손창
        if (lo.Weapon.WeaponClass == WeaponClass.Sword) return true; // 후열 침투
        return false;
    }

    // 공격 가능한 적 목록: 전열 + (후열 도달 가능하거나 전열이 비었을 때) 후열
    public static List<Combatant> Reachable(WeaponLoadout attacker, Formation enemy) {
        var result = new List<Combatant>(enemy.RowAlive(FormationRow.Front));
        bool frontEmpty = result.Count == 0;
        if (CanReachBackRow(attacker) || frontEmpty)
            result.AddRange(enemy.RowAlive(FormationRow.Back));
        return result;
    }

    // 주 타겟: 도달 가능한 적 중 현재 HP가 가장 낮은 유닛 (없으면 null)
    public static Combatant SelectPrimary(WeaponLoadout attacker, Formation enemy) {
        var reachable = Reachable(attacker, enemy);
        Combatant best = null;
        foreach (var c in reachable)
            if (best == null || c.CurrentHP < best.CurrentHP) best = c;
        return best;
    }

    // 주 타겟 + 무기 특성에 따른 추가 피격 대상 (주 타겟 포함).
    public static List<Combatant> SelectTargets(WeaponLoadout attacker, Formation enemy) {
        var targets = new List<Combatant>();
        var primary = SelectPrimary(attacker, enemy);
        if (primary == null) return targets;
        targets.Add(primary);

        var effect = attacker?.Weapon != null && attacker.IsTwoHand
            ? attacker.Weapon.TwoHandEffect
            : TwoHandEffect.None;

        switch (effect) {
            case TwoHandEffect.AoeRow: // 대상의 열 전체
                foreach (var c in enemy.RowAlive(primary.Row))
                    if (c != primary) targets.Add(c);
                break;
            case TwoHandEffect.Pierce: // 대상 바로 뒤(같은 열 후열)
                if (primary.Row == FormationRow.Front) {
                    var behind = enemy.At(FormationRow.Back, primary.Column);
                    if (behind != null && behind.IsAlive) targets.Add(behind);
                }
                break;
        }
        return targets;
    }

    // 공격자가 방어 무시(양손둔기)인가?
    public static bool IgnoresDefense(WeaponLoadout attacker) =>
        attacker?.Weapon != null && attacker.IsTwoHand &&
        attacker.Weapon.TwoHandEffect == TwoHandEffect.ArmorBreakKnockback;

    // 공격자가 넉백을 유발(양손둔기)하는가?
    public static bool CausesKnockback(WeaponLoadout attacker) => IgnoresDefense(attacker);
}
