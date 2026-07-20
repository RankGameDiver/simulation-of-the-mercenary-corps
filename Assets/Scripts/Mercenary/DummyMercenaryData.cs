public class DummyMercenaryData : MercenaryData
{
    private static readonly string[] Personalities = { "신중형", "충동형", "낙천형", "비관형", "지도자형", "학자형", "냉정형" };

    // 스탯 가중치 (합계 100 기준, 조정 시 합계가 100을 초과하지 않도록)
    private const int StatWeightAboveAverage = 10;  // 스탯 7
    private const int StatWeightGood         = 5;   // 스탯 8
    private const int StatWeightExcellent    = 3;   // 스탯 9
    private const int StatWeightPeak         = 1;   // 스탯 10

    public DummyMercenaryData(string name, string personality = null) {
        Id = System.Guid.NewGuid().ToString();
        Name = name;
        Stamina = 100;
        Morale = 100;
        PersonalityType = personality ?? Personalities[UnityEngine.Random.Range(0, Personalities.Length)];

        Strength     = RollStat();
        Agility      = RollStat();
        Health       = RollStat();
        Intelligence = RollStat();
        Charming     = RollStat();
    }

    public override bool IsIncapacitated => Stamina <= 0;

    public override void AssignAction(string actionId) {
        CurrentAction = actionId;
    }

    public override void OnTurnStart() {
        // 이전 턴 행동 초기화 (새 턴 계획을 위해 비움)
        CurrentAction = "대기";
    }

    public override void OnTurnExecute() {
        // 턴마다 기력 소모 시뮬레이션
        Stamina -= 10;

        // 행동 미지정 시 성격 기반 자율 행동
        if(CurrentAction == "대기") {
            CurrentAction = GetAutonomousAction();
        }
    }

    private string GetAutonomousAction() {
        return PersonalityType switch {
            "신중형"  => "훈련",
            "충동형"  => "자율 출정 시도",
            "낙천형"  => "시장 방문",
            "비관형"  => "휴식",
            "지도자형" => "토론 주도",
            "학자형"  => "독서",
            "냉정형"  => "전술 분석",
            _         => "휴식"
        };
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