public class DummyMercenaryData : MercenaryData
{
    // 스탯 가중치 (합계 100 기준, 조정 시 합계가 100을 초과하지 않도록)
    private const int StatWeightAboveAverage = 10;  // 스탯 7
    private const int StatWeightGood         = 5;   // 스탯 8
    private const int StatWeightExcellent    = 3;   // 스탯 9
    private const int StatWeightPeak         = 1;   // 스탯 10

    private readonly ActionDefinition _defaultAction; // 행동 미지정 시 자율 행동

    public DummyMercenaryData(string name, ActionCatalog catalog) {
        Id = System.Guid.NewGuid().ToString();
        Name = name;
        Stamina = 100;
        Morale = 100;
        _defaultAction = catalog.DefaultAction;

        Strength     = RollStat();
        Agility      = RollStat();
        Health       = RollStat();
        Intelligence = RollStat();
        Charming     = RollStat();

        RollUnlockedActions(catalog);
    }

    // 용병 생성 시 각 행동의 해금 여부를 확률로 결정
    private void RollUnlockedActions(ActionCatalog catalog) {
        foreach (var action in catalog.Actions) {
            if (action == null) continue;
            // 확률이 1이면 항상 해금 (Random.value는 1.0을 포함할 수 있어 명시적으로 처리)
            bool unlocked = action.UnlockChance >= 1f || UnityEngine.Random.value < action.UnlockChance;
            if (unlocked) unlockedActions.Add(action);
        }
        // 자율 기본 행동은 항상 수행 가능해야 하므로 보장
        if (_defaultAction != null) unlockedActions.Add(_defaultAction);
    }

    public override bool IsIncapacitated => Stamina <= 0;

    public override void AssignAction(ActionDefinition action) {
        CurrentAction = action;
    }

    public override void OnTurnStart() {
        // 이전 턴 행동 초기화 (새 턴 계획을 위해 비움)
        CurrentAction = null;
    }

    public override void OnTurnExecute() {
        // 턴마다 기력 소모 시뮬레이션
        Stamina -= 10;

        // 행동 미지정 시 기본 자율 행동
        if(CurrentAction == null) {
            CurrentAction = _defaultAction;
        }
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