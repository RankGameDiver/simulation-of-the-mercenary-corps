using System.Collections.Generic;
using UnityEngine;

// 하나의 전투 조우: 적 구성과 초기 포메이션 배치(전열/후열 × 3칸).
[CreateAssetMenu(fileName = "새 조우", menuName = "용병단/조우 정의")]
public class EncounterDefinition : ScriptableObject
{
    [System.Serializable]
    public struct Placement {
        public EnemyUnitDefinition unit;
        public FormationRow row;
        [Range(0, 2)] public int column;
    }

    [SerializeField] private string displayName = "조우";
    [SerializeField] private Placement[] enemies;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public IReadOnlyList<Placement> Enemies => enemies;
}
