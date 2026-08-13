using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 턴제 전투를 조율하는 싱글톤. 관리용 TurnManager와 분리되어 있으며 전투 조우만 담당한다.
// 규칙 계산은 CombatMath/WeaponMatchup/CombatTargeting(순수 규칙)에 위임하고,
// 진행 상황은 GameEvents로 방송한다(시스템 직접 참조 금지 규칙 준수).
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("규칙 애셋")]
    [SerializeField] private ProficiencyCurve proficiencyCurve;
    [SerializeField] private CombatBalanceConfig balance;

    [Header("연출/안전장치")]
    [SerializeField] private float actionDelay = 0.4f;       // 행동 사이 딜레이
    [SerializeField, Range(0f, 1f)] private float baseHitChance = 0.9f;
    [SerializeField] private int maxRounds = 30;             // 무한 루프 방지

    public bool IsInCombat { get; private set; }

    private Formation _playerFormation;
    private Formation _enemyFormation;

    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 전투 시작. party(아군 용병)와 encounter(적 구성)로 양측 포메이션을 구성한다.
    public void StartEncounter(IReadOnlyList<IMercenary> party, EncounterDefinition encounter) {
        if (IsInCombat) { GameEvents.Log("[전투] 이미 전투 중입니다"); return; }
        if (proficiencyCurve == null || balance == null) {
            GameEvents.Log("[전투] 규칙 애셋(숙련도 곡선/전투 밸런스 설정)이 연결되지 않았습니다");
            return;
        }
        if (encounter == null) { GameEvents.Log("[전투] 조우 정의가 없습니다"); return; }

        _playerFormation = BuildPlayerFormation(party);
        _enemyFormation = BuildEnemyFormation(encounter);

        if (!_playerFormation.HasAnyAlive() || !_enemyFormation.HasAnyAlive()) {
            GameEvents.Log("[전투] 한쪽 진영이 비어 전투를 시작할 수 없습니다");
            return;
        }

        StartCoroutine(RunCombat());
    }

    // 방패/둔기는 전열, 그 외(검·창·맨손 유격)는 후열에 배치
    private Formation BuildPlayerFormation(IReadOnlyList<IMercenary> party) {
        var f = new Formation();
        int frontCol = 0, backCol = 0;
        if (party != null) {
            foreach (var m in party) {
                if (m == null) continue;
                var c = Combatant.FromMercenary(m);
                if (PrefersFront(c.Loadout)) f.Place(c, FormationRow.Front, frontCol++);
                else f.Place(c, FormationRow.Back, backCol++);
            }
        }
        return f;
    }

    private static bool PrefersFront(WeaponLoadout lo) {
        if (lo?.Weapon == null) return true;               // 맨손은 일단 전열
        if (lo.Grip == GripMode.OneHandShield) return true; // 방패는 전열 고정형
        return lo.Weapon.WeaponClass == WeaponClass.Blunt;  // 둔기는 전열 붕괴형
    }

    private Formation BuildEnemyFormation(EncounterDefinition encounter) {
        var f = new Formation();
        if (encounter.Enemies != null)
            foreach (var p in encounter.Enemies) {
                if (p.unit == null) continue;
                f.Place(Combatant.FromEnemy(p.unit), p.row, p.column);
            }
        return f;
    }

    private IEnumerator RunCombat() {
        IsInCombat = true;
        GameEvents.CombatStarted();
        GameEvents.Log("=== 전투 시작 ===");

        int round = 0;
        while (_playerFormation.HasAnyAlive() && _enemyFormation.HasAnyAlive() && round < maxRounds) {
            round++;
            GameEvents.Log($"--- {round}라운드 ---");

            foreach (var actor in BuildInitiativeOrder()) {
                if (!actor.IsAlive) continue;
                if (!_playerFormation.HasAnyAlive() || !_enemyFormation.HasAnyAlive()) break;

                var enemyFormation = actor.IsPlayer ? _enemyFormation : _playerFormation;
                PerformAttack(actor, enemyFormation);
                yield return new WaitForSeconds(actionDelay);
            }
        }

        bool playerWon = _playerFormation.HasAnyAlive() && !_enemyFormation.HasAnyAlive();
        bool playerLost = !_playerFormation.HasAnyAlive() && _enemyFormation.HasAnyAlive();
        GameEvents.Log(playerWon ? "=== 전투 승리 ==="
                      : playerLost ? "=== 전투 패배 ==="
                      : "=== 전투 종료 (시간 초과) ===");
        GameEvents.CombatEnded(playerWon);
        IsInCombat = false;
    }

    // 양측 생존 유닛을 행동속도(민첩 × 운용 속도배수) 내림차순 정렬
    private List<Combatant> BuildInitiativeOrder() {
        var all = new List<Combatant>();
        all.AddRange(_playerFormation.AllAlive());
        all.AddRange(_enemyFormation.AllAlive());
        all.Sort((a, b) => Speed(b).CompareTo(Speed(a)));
        return all;
    }

    private float Speed(Combatant c) {
        float mult = c.Loadout != null ? CombatMath.SpeedMultiplier(c.Loadout, balance) : 1f;
        return c.Agility * mult;
    }

    private void PerformAttack(Combatant attacker, Formation enemyFormation) {
        var targets = attacker.Loadout != null
            ? CombatTargeting.SelectTargets(attacker.Loadout, enemyFormation)
            : SingleFrontTarget(enemyFormation);
        if (targets.Count == 0) return;

        var primary = targets[0];

        // 명중 판정: 기본 + 숙련·운용 명중 − 대상 회피
        float hit = baseHitChance
            + (attacker.Loadout != null ? CombatMath.HitModifier(attacker.Loadout, proficiencyCurve, balance) : 0f)
            - (primary.Loadout != null ? CombatMath.EvasionModifier(primary.Loadout, balance) : 0f);
        if (Random.value > Mathf.Clamp01(hit)) {
            GameEvents.Log($"[{attacker.Name}] → [{primary.Name}] 빗나감");
            GameEvents.CombatantActed(attacker);
            return;
        }

        foreach (var target in targets)
            if (target.IsAlive)
                ApplyDamage(attacker, target, isPrimary: target == primary);

        GameEvents.CombatantActed(attacker);
    }

    private void ApplyDamage(Combatant attacker, Combatant target, bool isPrimary) {
        var atk = attacker.Loadout;
        float effective = atk != null
            ? CombatMath.EffectiveAttack(attacker.Strength, attacker.Agility, atk, proficiencyCurve)
            : attacker.Strength; // 맨손 최소치

        var matchup = MatchupResult.Neutral;
        if (atk?.Weapon != null && target.Loadout?.Weapon != null)
            matchup = WeaponMatchup.Evaluate(atk.Weapon.WeaponClass, target.Loadout.Weapon.WeaponClass, balance);

        bool ignoreDef = CombatTargeting.IgnoresDefense(atk);
        float reduction = CombatMath.DamageReduction(target.Loadout, proficiencyCurve, balance);
        bool twoHand = atk != null && atk.IsTwoHand;

        float dmg = CombatMath.ResolveDamage(effective, matchup, twoHand, ignoreDef, reduction, balance);
        target.TakeDamage(dmg);

        string tag = matchup == MatchupResult.Advantage ? " (상성 유리)"
                   : matchup == MatchupResult.Disadvantage ? " (상성 불리)" : "";
        GameEvents.Log($"[{attacker.Name}] → [{target.Name}] {Mathf.RoundToInt(dmg)} 피해{tag} (HP {target.CurrentHP}/{target.MaxHP})");

        if (!target.IsAlive) {
            GameEvents.Log($"[{target.Name}] 쓰러짐");
            return;
        }

        // 넉백(양손둔기): 주 타겟을 후열로 밀어냄
        if (isPrimary && CombatTargeting.CausesKnockback(atk)) {
            var formation = target.IsPlayer ? _playerFormation : _enemyFormation;
            if (formation.Knockback(target))
                GameEvents.Log($"[{target.Name}] 넉백 — 후열로 밀려남");
        }
    }

    private static List<Combatant> SingleFrontTarget(Formation enemy) {
        var front = enemy.RowAlive(FormationRow.Front);
        if (front.Count > 0) return new List<Combatant> { front[0] };
        var back = enemy.RowAlive(FormationRow.Back);
        return back.Count > 0 ? new List<Combatant> { back[0] } : new List<Combatant>();
    }
}
