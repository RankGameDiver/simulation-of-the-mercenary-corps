// 무기 상성 판정 (순수 규칙). 상성 고리: 창 > 검 > 둔기 > 창.
// 양손무기는 상성 편차를 증폭받아 하이리스크 하이리턴이 된다.
public static class WeaponMatchup
{
    // 공격자 무기가 방어자 무기에 대해 유리/중립/불리인지 판정
    public static MatchupResult Evaluate(WeaponClass attacker, WeaponClass defender, CombatBalanceConfig cfg) {
        if (cfg == null || attacker == defender) return MatchupResult.Neutral;
        if (cfg.IsAdvantage(attacker, defender)) return MatchupResult.Advantage;
        if (cfg.IsAdvantage(defender, attacker)) return MatchupResult.Disadvantage;
        return MatchupResult.Neutral;
    }

    // 상성 결과에 따른 피해 배율. 양손이면 1로부터의 편차를 twoHandMatchupAmplify배 증폭.
    public static float Multiplier(MatchupResult result, bool attackerTwoHand, CombatBalanceConfig cfg) {
        float baseMult = result switch {
            MatchupResult.Advantage => cfg.AdvantageMultiplier,
            MatchupResult.Disadvantage => cfg.DisadvantageMultiplier,
            _ => 1f,
        };

        if (!attackerTwoHand || result == MatchupResult.Neutral) return baseMult;

        // 유리 1.2 → 편차 0.2 → 1 + 0.2*1.5 = 1.3 (+30%)
        // 불리 0.8 → 편차 -0.2 → 1 + (-0.2*1.5) = 0.7 (-30%)
        float deviation = baseMult - 1f;
        return 1f + deviation * cfg.TwoHandMatchupAmplify;
    }
}
