using UnityEngine;

// 하나의 전투 조우: 1대1로 맞붙을 적 한 명.
[CreateAssetMenu(fileName = "NewEncounter", menuName = "용병단/조우 정의")]
public class EncounterDefinition : ScriptableObject
{
    [SerializeField] private string displayName = "조우";
    [SerializeField] private EnemyUnitDefinition enemy; // 상대할 적

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public EnemyUnitDefinition Enemy => enemy;
}
