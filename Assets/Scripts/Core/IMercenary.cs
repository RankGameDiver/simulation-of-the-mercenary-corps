// 턴/UI 시스템이 용병을 바라보는 창구
// 데이터 구조가 변경되어도 이 인터페이스만 유지하면 됨
public interface IMercenary
{
    string Id { get; }
    string Name { get; }
    int Stamina { get; }
    int Morale { get; }
    int Strength { get; }      // 힘 (1–10)
    int Agility { get; }       // 민첩 (1–10)
    int Intelligence { get; }  // 지능 (1–10)
    int Charming { get; }      // 매력 (1–10)
    string PersonalityType { get; }
    string CurrentAction { get; }
    bool IsIncapacitated { get; }

    void AssignAction(string actionId); // 작업 할당
    void OnTurnExecute(); // 턴 실행 시 호출
}