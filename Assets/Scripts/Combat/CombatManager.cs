using UnityEngine;

// 1대1 턴제 전투를 조율하는 싱글톤. 관리용 TurnManager와 분리되어 있으며 전투 조우만 담당한다.
// 규칙 계산은 CombatMath/WeaponMatchup(순수 규칙)에 위임하고,
// 진행 상황은 GameEvents로 방송한다(시스템 직접 참조 금지 규칙 준수).
//
// 현재 상태: 다대다 자동 전투를 걷어낸 골격. 양측 전투 유닛 구성과 일반 공격 판정만 남아 있다.
// 라운드 루프(동시 선택 → 해결)와 방어/대기/스킬은 기획서 v2 5장에 따라 구현 예정.
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("규칙 애셋")]
    [SerializeField] private ProficiencyCurve proficiencyCurve;
    [SerializeField] private CombatBalanceConfig balance;

    [Header("판정")]
    [SerializeField, Range(0f, 1f)] private float baseHitChance = 0.9f;

    public bool IsInCombat { get; private set; }

    // 1대1 전투의 양측 유닛 (전투 중이 아니면 null일 수 있음)
    public Combatant Player { get; private set; }
    public Combatant Enemy { get; private set; }

    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 전투 시작. 관리 중인 용병과 조우의 적으로 1대1 유닛을 구성한다.
    public void StartEncounter(IMercenary player, EncounterDefinition encounter) {
        if (IsInCombat) { GameEvents.Log("[전투] 이미 전투 중입니다"); return; }
        if (proficiencyCurve == null || balance == null) {
            GameEvents.Log("[전투] 규칙 애셋(숙련도 곡선/전투 밸런스 설정)이 연결되지 않았습니다");
            return;
        }
        if (player == null) { GameEvents.Log("[전투] 전투에 나설 용병이 없습니다"); return; }
        if (encounter == null || encounter.Enemy == null) { GameEvents.Log("[전투] 조우 정의(상대 적)가 없습니다"); return; }

        Player = Combatant.FromMercenary(player);
        Enemy = Combatant.FromEnemy(encounter.Enemy);

        GameEvents.Log($"[전투] {Player.Name} vs {Enemy.Name} — 라운드 진행은 아직 구현되지 않았습니다");
    }

    // 일반 공격 1회: 명중 판정 후 피해 적용 (라운드 루프 구현 시 '공격' 행동에서 사용)
    private void PerformAttack(Combatant attacker, Combatant defender) {
        // 명중 판정: 기본 + 숙련·운용 명중 − 대상 회피
        float hit = baseHitChance
            + (attacker.Loadout != null ? CombatMath.HitModifier(attacker.Loadout, proficiencyCurve, balance) : 0f)
            - (defender.Loadout != null ? CombatMath.EvasionModifier(defender.Loadout, balance) : 0f);
        if (Random.value > Mathf.Clamp01(hit)) {
            GameEvents.Log($"[{attacker.Name}] → [{defender.Name}] 빗나감");
            GameEvents.CombatantActed(attacker);
            return;
        }

        var atk = attacker.Loadout;
        float effective = atk != null
            ? CombatMath.EffectiveAttack(attacker.Strength, attacker.Agility, atk, proficiencyCurve)
            : attacker.Strength; // 맨손 최소치

        var matchup = MatchupResult.Neutral;
        if (atk?.Weapon != null && defender.Loadout?.Weapon != null)
            matchup = WeaponMatchup.Evaluate(atk.Weapon.WeaponClass, defender.Loadout.Weapon.WeaponClass, balance);

        // 양손둔기는 방어 무시
        bool ignoreDef = atk?.Weapon != null && atk.IsTwoHand
                         && atk.Weapon.TwoHandEffect == TwoHandEffect.ArmorBreakKnockback;
        float reduction = CombatMath.DamageReduction(defender.Loadout, proficiencyCurve, balance);
        bool twoHand = atk != null && atk.IsTwoHand;

        float dmg = CombatMath.ResolveDamage(effective, matchup, twoHand, ignoreDef, reduction, balance);
        defender.TakeDamage(dmg);

        string tag = matchup == MatchupResult.Advantage ? " (상성 유리)"
                   : matchup == MatchupResult.Disadvantage ? " (상성 불리)" : "";
        GameEvents.Log($"[{attacker.Name}] → [{defender.Name}] {Mathf.RoundToInt(dmg)} 피해{tag} (HP {defender.CurrentHP}/{defender.MaxHP})");
        if (!defender.IsAlive) GameEvents.Log($"[{defender.Name}] 쓰러짐");

        GameEvents.CombatantActed(attacker);
    }
}
