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
    public string PersonalityType { get; protected set; }
    public string CurrentAction { get; protected set; } = "대기";
    public virtual bool IsIncapacitated => Stamina <= 0;

    public virtual void AssignAction(string actionId) { CurrentAction = actionId; }
    public virtual void OnTurnStart() { }
    public virtual void OnTurnExecute() { }
}