using UnityEngine;

// 적 유닛 1기의 정의 애셋.
[CreateAssetMenu(fileName = "새 적", menuName = "용병단/적 유닛 정의")]
public class EnemyUnitDefinition : ScriptableObject
{
    [SerializeField] private string displayName = "적";
    [SerializeField] private int maxHP = 40;
    [SerializeField, Range(1, 10)] private int strength = 5;
    [SerializeField, Range(1, 10)] private int agility = 5;

    [Header("무장")]
    [SerializeField] private WeaponDefinition weapon;
    [SerializeField] private GripMode grip = GripMode.OneHand;
    [SerializeField, Range(0, 5)] private int weaponLevel = 1;
    [SerializeField, Range(0, 5)] private int shieldLevel = 0;

    [Header("배치 선호")]
    [SerializeField] private FormationRow preferredRow = FormationRow.Front;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public int MaxHP => maxHP;
    public int Strength => strength;
    public int Agility => agility;
    public FormationRow PreferredRow => preferredRow;

    public WeaponLoadout BuildLoadout() {
        if (weapon == null) return null;
        var lo = new WeaponLoadout(weapon, grip);
        lo.SetLevel(weapon.GoverningProficiency, weaponLevel);
        if (shieldLevel > 0) lo.SetLevel(Proficiency.ShieldArts, shieldLevel);
        return lo;
    }
}
