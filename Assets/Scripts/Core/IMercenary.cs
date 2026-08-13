using System.Collections.Generic;

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
    int Health { get; }       // 건강 (1–10)
    int Intelligence { get; }  // 지능 (1–10)
    int Charming { get; }      // 매력 (1–10)
    ActionDefinition CurrentAction { get; } // null이면 미지정(대기)
    bool IsIncapacitated { get; }

    WeaponLoadout Loadout { get; } // 장착 무기·운용·숙련도 (없으면 null)

    IReadOnlyCollection<ActionDefinition> UnlockedActions { get; } // 수행 가능한(해금된) 행동
    bool IsActionUnlocked(ActionDefinition action);

    void AssignAction(ActionDefinition action); // 작업 할당
    void OnTurnStart(); // 턴 시작 시 호출
    void OnTurnExecute(); // 턴 실행 시 호출
}