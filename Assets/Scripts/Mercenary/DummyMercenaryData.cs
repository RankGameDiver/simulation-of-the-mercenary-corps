public class DummyMercenaryData : IMercenary
{
    public string Id { get; }
    public string Name { get; }
    public int Stamina { get; private set; }
    public int Morale { get; private set; }
    public string PersonalityType { get; }
    public string CurrentAction { get; private set; } = "대기";
    public bool IsIncapacitated => Stamina <= 0f;

    private static readonly string[] Personalities = { "신중형", "충동형", "낙천형", "비관형", "지도자형", "학자형", "냉정형" };

    public DummyMercenaryData(string name, string personality = null) {
        Id = System.Guid.NewGuid().ToString();
        Name = name;
        Stamina = 100;
        Morale = 100;
        PersonalityType = personality ?? Personalities[UnityEngine.Random.Range(0, Personalities.Length)];
    }

    public void AssignAction(string actionId) {
        CurrentAction = actionId;
    }

    public void OnTurnExecute() {
        // 턴마다 기력 소모 시뮬레이션
        Stamina -= 10;

        // 행동 미지정 시 성격 기반 자율 행동
        if(CurrentAction == "대기") {
            CurrentAction = GetAutonomousAction();
        }

        // 다음 턴 초기화
        var executed = CurrentAction;
        CurrentAction = "대기";
        _ = executed; // 로그는 TurnManager가 처리
    }

    private string GetAutonomousAction() {
        return PersonalityType switch {
            "신중형" => "훈련",
            "충동형" => "자율 출정 시도",
            "낙천형" => "시장 방문",
            "비관형" => "휴식",
            "지도자형" => "토론 주도",
            "학자형" => "독서",
            "냉정형" => "전술 분석",
            _ => "휴식"
        };
    }
}