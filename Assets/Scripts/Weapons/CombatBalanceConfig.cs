using System.Collections.Generic;
using UnityEngine;

// 전투 밸런싱 수치의 단일 원천. 문서 권고("수치는 프로토타입 때 조정")에 따라
// 상성 배율·운용 효과·양손 페널티·초기 롤 가중치를 전부 에디터 노출한다.
[CreateAssetMenu(fileName = "전투 밸런스 설정", menuName = "용병단/전투 밸런스 설정")]
public class CombatBalanceConfig : ScriptableObject
{
    [System.Serializable]
    public struct Matchup {
        public WeaponClass attacker; // 공격 무기 클래스
        public WeaponClass defender; // 이 클래스에 유리
    }

    [Header("상성 배율")]
    [SerializeField] private float advantageMultiplier = 1.2f;    // 유리 +20%
    [SerializeField] private float disadvantageMultiplier = 0.8f; // 불리 -20%
    [Tooltip("양손무기는 상성 편차를 이 배수만큼 증폭 (1.5 → 유리 +30%/불리 -30%)")]
    [SerializeField] private float twoHandMatchupAmplify = 1.5f;

    [Header("양손 공통 페널티")]
    [SerializeField] private float twoHandSpeedPenalty = -0.2f;      // 행동속도 -20%
    [SerializeField] private float twoHandStaminaMultiplier = 1.5f;  // 기력 소모 x1.5

    [Header("운용: 한손 단독 (오프핸드)")]
    [SerializeField] private float offhandHitBonus = 0.1f;         // 명중 +10%
    [SerializeField] private float offhandEvasionBonus = 0.1f;     // 회피 +10%
    [SerializeField] private float offhandSpeedBonus = 0.1f;       // 행동속도 +10%
    [SerializeField] private float offhandStaminaReduction = 0.2f; // 기력 소모 -20%
    [SerializeField] private int offhandMoveBonus = 1;             // 이동력 +1

    [Header("운용: 한손+방패 (방패술 숙련도에 비례)")]
    [SerializeField] private float shieldDamageReductionMin = 0.15f; // 방패술 0일 때
    [SerializeField] private float shieldDamageReductionMax = 0.25f; // 방패술 최대일 때
    [SerializeField] private float shieldSpeedPenalty = -0.1f;       // 행동속도 -10%

    [Header("초기 숙련도 롤 가중치 (Lv.1~5, 높을수록 희소)")]
    [SerializeField] private int[] levelRollWeights = { 40, 25, 18, 12, 5 };

    [Header("상성 고리 (공격 클래스가 방어 클래스에 유리)")]
    [SerializeField] private List<Matchup> advantages = new() {
        new Matchup { attacker = WeaponClass.Spear, defender = WeaponClass.Sword }, // 창 > 검
        new Matchup { attacker = WeaponClass.Sword, defender = WeaponClass.Blunt }, // 검 > 둔기
        new Matchup { attacker = WeaponClass.Blunt, defender = WeaponClass.Spear }, // 둔기 > 창
    };

    public float AdvantageMultiplier => advantageMultiplier;
    public float DisadvantageMultiplier => disadvantageMultiplier;
    public float TwoHandMatchupAmplify => twoHandMatchupAmplify;
    public float TwoHandSpeedPenalty => twoHandSpeedPenalty;
    public float TwoHandStaminaMultiplier => twoHandStaminaMultiplier;
    public float OffhandHitBonus => offhandHitBonus;
    public float OffhandEvasionBonus => offhandEvasionBonus;
    public float OffhandSpeedBonus => offhandSpeedBonus;
    public float OffhandStaminaReduction => offhandStaminaReduction;
    public int OffhandMoveBonus => offhandMoveBonus;
    public float ShieldSpeedPenalty => shieldSpeedPenalty;
    public IReadOnlyList<int> LevelRollWeights => levelRollWeights;

    // 방패 피해 감소율: 방패술 숙련도(0~maxLevel)에 비례해 min~max 보간
    public float ShieldDamageReduction(int shieldLevel, int maxLevel) {
        if (maxLevel <= 0) return shieldDamageReductionMin;
        float t = Mathf.Clamp01((float)shieldLevel / maxLevel);
        return Mathf.Lerp(shieldDamageReductionMin, shieldDamageReductionMax, t);
    }

    // 공격 클래스가 방어 클래스에 유리한가?
    public bool IsAdvantage(WeaponClass attacker, WeaponClass defender) {
        foreach (var m in advantages)
            if (m.attacker == attacker && m.defender == defender) return true;
        return false;
    }
}
