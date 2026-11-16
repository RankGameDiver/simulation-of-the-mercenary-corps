using System;
using System.Collections.Generic;

// 진행 중인 게임 한 판의 저장 데이터 (JsonUtility 직렬화 대상).
// 행동·무기 애셋은 애셋 이름으로 저장하고, 불러올 때 카탈로그에서 다시 찾는다.
// 형식을 바꾸면 CurrentVersion을 올린다. 버전이 다른 세이브는 불러오지 않는다.
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public string savedAtUtc; // ISO 8601, 표시·디버깅용
    public int turn;          // TurnManager.CurrentTurn (0부터, 표시는 +1)
    public MercenarySaveData mercenary;
}

// 용병 한 명의 저장 데이터. 일과는 매 턴 새로 고르므로 저장하지 않는다.
[Serializable]
public class MercenarySaveData
{
    public string id;
    public string name;
    public int stamina;
    public int strength;
    public int agility;
    public int health;
    public int intelligence;
    public int charming;
    public int currentHp;

    public List<string> unlockedActions = new(); // ActionDefinition 애셋 이름
    public string weapon;                        // WeaponDefinition 애셋 이름, 빈 문자열이면 무기 없음
    public GripMode grip;
    public List<ProficiencyLevelSave> proficiencies = new();
}

[Serializable]
public class ProficiencyLevelSave
{
    public Proficiency proficiency;
    public int level;
}
