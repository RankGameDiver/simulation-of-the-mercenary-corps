using System.Collections.Generic;

public class MercenaryData : IMercenary
{
    public string Id { get; protected set; }
    public string Name { get; protected set; }
    public int Stamina { get; protected set; }
    public int Morale { get; protected set; }
    public int Strength { get; protected set; }      // 근력 (1–10)
    public int Agility { get; protected set; }       // 민첩 (1–10)
    public int Health { get; protected set; }       // 체력 (1–10)
    public int Intelligence { get; protected set; }  // 지능 (1–10)
    public int Charming { get; protected set; }      // 매력 (1–10)
    public ActionDefinition CurrentAction { get; protected set; } // null = 미지정(대기)
    public virtual bool IsIncapacitated => Stamina <= 0;

    // 이 용병이 수행할 수 있는(해금된) 행동 집합
    protected readonly HashSet<ActionDefinition> unlockedActions = new();
    public IReadOnlyCollection<ActionDefinition> UnlockedActions => unlockedActions;
    public bool IsActionUnlocked(ActionDefinition action) => action != null && unlockedActions.Contains(action);

    public virtual void AssignAction(ActionDefinition action) { CurrentAction = action; }
    public virtual void OnTurnStart() { }
    public virtual void OnTurnExecute() { }
}