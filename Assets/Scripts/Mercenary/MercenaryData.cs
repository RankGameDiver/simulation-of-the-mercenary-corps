using System.Collections.Generic;

public class MercenaryData : IMercenary
{
    public const int HpPerHealth = 10; // 최대 HP = 건강 × 10 (기획서 2.2 [제안])

    public string Id { get; protected set; }
    public string Name { get; protected set; }
    public int Stamina { get; protected set; }
    public int Strength { get; protected set; }      // 근력 (1–10)
    public int Agility { get; protected set; }       // 민첩 (1–10)
    public int Health { get; protected set; }       // 체력 (1–10)
    public int Intelligence { get; protected set; }  // 지능 (1–10)
    public int Charming { get; protected set; }      // 매력 (1–10)

    public int MaxHp => Health * HpPerHealth;
    public int CurrentHp { get; protected set; }

    public ActionDefinition CurrentAction { get; protected set; } // null = 미지정

    public WeaponLoadout Loadout { get; protected set; } // 장착 무기·운용·숙련도 (없으면 null)

    // 이 용병이 수행할 수 있는(해금된) 행동 집합
    protected readonly HashSet<ActionDefinition> unlockedActions = new();
    public IReadOnlyCollection<ActionDefinition> UnlockedActions => unlockedActions;
    public bool IsActionUnlocked(ActionDefinition action) => action != null && unlockedActions.Contains(action);

    public virtual void AssignAction(ActionDefinition action) { CurrentAction = action; }
    public virtual void OnTurnStart() { }
    public virtual void OnTurnExecute() { }

    // ── 저장·복원 ──

    public MercenarySaveData ToSaveData() {
        var data = new MercenarySaveData {
            id = Id,
            name = Name,
            stamina = Stamina,
            strength = Strength,
            agility = Agility,
            health = Health,
            intelligence = Intelligence,
            charming = Charming,
            currentHp = CurrentHp,
            weapon = Loadout?.Weapon != null ? Loadout.Weapon.name : "",
            grip = Loadout?.Grip ?? GripMode.OneHand,
        };

        foreach(var action in unlockedActions) {
            if(action != null) data.unlockedActions.Add(action.name);
        }
        if(Loadout != null) {
            foreach(var pair in Loadout.Levels) {
                data.proficiencies.Add(new ProficiencyLevelSave { proficiency = pair.Key, level = pair.Value });
            }
        }
        return data;
    }

    // 저장된 값으로 덮어쓴다. 카탈로그에서 찾지 못한 애셋(이름 변경·삭제)은 경고를 남기고 건너뛴다.
    protected void ApplySaveData(MercenarySaveData data, ActionCatalog actions, WeaponCatalog weapons) {
        Id = data.id;
        Name = data.name;
        Stamina = data.stamina;
        Strength = data.strength;
        Agility = data.agility;
        Health = data.health;
        Intelligence = data.intelligence;
        Charming = data.charming;
        CurrentHp = System.Math.Clamp(data.currentHp, 0, MaxHp);
        CurrentAction = null; // 일과는 매 턴 새로 고른다

        unlockedActions.Clear();
        foreach(var actionName in data.unlockedActions) {
            var action = actions != null ? actions.FindByName(actionName) : null;
            if(action != null) unlockedActions.Add(action);
            else UnityEngine.Debug.LogWarning($"[세이브] 행동 '{actionName}'을(를) 카탈로그에서 찾지 못해 건너뜁니다.");
        }

        Loadout = null;
        if(!string.IsNullOrEmpty(data.weapon)) {
            var weapon = weapons != null ? weapons.FindByName(data.weapon) : null;
            if(weapon == null) {
                UnityEngine.Debug.LogWarning($"[세이브] 무기 '{data.weapon}'을(를) 카탈로그에서 찾지 못해 무기 없이 불러옵니다.");
                return;
            }
            var loadout = new WeaponLoadout(weapon, data.grip);
            foreach(var entry in data.proficiencies) loadout.SetLevel(entry.proficiency, entry.level);
            Loadout = loadout;
        }
    }
}
