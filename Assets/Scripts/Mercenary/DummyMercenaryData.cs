public class DummyMercenaryData : MercenaryData
{
    // 스탯 가중치 (합계 100 기준, 조정 시 합계가 100을 초과하지 않도록)
    private const int StatWeightAboveAverage = 10;  // 스탯 7
    private const int StatWeightGood         = 5;   // 스탯 8
    private const int StatWeightExcellent    = 3;   // 스탯 9
    private const int StatWeightPeak         = 1;   // 스탯 10

    public DummyMercenaryData(string name, ActionCatalog catalog,
                              WeaponCatalog weapons = null, CombatBalanceConfig balance = null) {
        Id = System.Guid.NewGuid().ToString();
        Name = name;
        Stamina = 100;

        Strength     = RollStat();
        Agility      = RollStat();
        Health       = RollStat();
        Intelligence = RollStat();
        Charming     = RollStat();
        CurrentHp    = MaxHp; // 생성 직후에는 가득 찬 상태

        RollUnlockedActions(catalog);
        RollSingleWeaponSpecialization(weapons, balance);
    }

    // 단일 무기 특화: 무기 하나를 골라 그 지배 숙련만 가중 롤(Lv.1~5, 높을수록 희소),
    // 나머지 숙련은 0. 운용 기본값은 한손 단독.
    private void RollSingleWeaponSpecialization(WeaponCatalog weapons, CombatBalanceConfig balance) {
        var weapon = weapons != null ? weapons.RandomWeapon() : null;
        if (weapon == null) { Loadout = null; return; }

        var weights = balance != null ? balance.LevelRollWeights : null;
        int level = WeaponRoll.WeightedLevel(weights);

        var loadout = new WeaponLoadout(weapon, GripMode.OneHand);
        loadout.SetLevel(weapon.GoverningProficiency, level);
        Loadout = loadout;
    }

    // 용병 생성 시 각 행동의 해금 여부를 확률로 결정
    private void RollUnlockedActions(ActionCatalog catalog) {
        foreach (var action in catalog.Actions) {
            if (action == null) continue;
            // 확률이 1이면 항상 해금 (Random.value는 1.0을 포함할 수 있어 명시적으로 처리)
            bool unlocked = action.UnlockChance >= 1f || UnityEngine.Random.value < action.UnlockChance;
            if (unlocked) unlockedActions.Add(action);
        }
        // 일과는 직접 골라야만 턴을 넘길 수 있으므로, 해금된 일과가 하나도 없으면 진행이 막힌다.
        // 최소 하나는 unlockChance = 1 로 두어야 한다.
        if (unlockedActions.Count == 0 && catalog.Actions.Count > 0)
            UnityEngine.Debug.LogWarning($"[용병 생성] {Name}: 해금된 일과가 없습니다. 행동 카탈로그에 unlockChance = 1인 일과를 두세요.");
    }

    public override void AssignAction(ActionDefinition action) {
        // 해금되지 않은 일과는 지정할 수 없다
        if (action != null && !IsActionUnlocked(action)) return;
        CurrentAction = action;
    }

    public override void OnTurnStart() {
        // 이전 턴 일과 초기화 (새 턴에는 다시 직접 골라야 한다)
        CurrentAction = null;
    }

    public override void OnTurnExecute() {
        // 일과별 효과(기력·숙련·자원 변화)는 아직 구현되지 않았다 (UI 문서 6장 2단계)
    }

    private static int RollStat() {
        int roll = UnityEngine.Random.Range(0, 100); // 0–99
        if (roll < StatWeightPeak)                                                                    return 10;
        if (roll < StatWeightPeak + StatWeightExcellent)                                              return 9;
        if (roll < StatWeightPeak + StatWeightExcellent + StatWeightGood)                             return 8;
        if (roll < StatWeightPeak + StatWeightExcellent + StatWeightGood + StatWeightAboveAverage)    return 7;
        // 나머지 81칸을 스탯 1–6에 균등 분배
        int lowRoll   = roll - (StatWeightPeak + StatWeightExcellent + StatWeightGood + StatWeightAboveAverage);
        int lowRange  = 100  - (StatWeightPeak + StatWeightExcellent + StatWeightGood + StatWeightAboveAverage);
        return lowRoll * 6 / lowRange + 1;
    }
}