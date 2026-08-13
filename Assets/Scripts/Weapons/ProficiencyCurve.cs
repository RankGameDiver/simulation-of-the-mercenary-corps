using UnityEngine;

// 숙련도 레벨(1~5) 곡선과 해금 게이트를 정의하는 애셋. 무기 공통으로 하나만 둔다.
// 문서 기준: Lv1 +3%/+5%, Lv2 +6%/+10%, Lv3 +9%/+15%(+양손 해금),
//            Lv4 +12%/+20%, Lv5 +15%/+25%(+필살기 해금).
[CreateAssetMenu(fileName = "숙련도 곡선", menuName = "용병단/숙련도 곡선")]
public class ProficiencyCurve : ScriptableObject
{
    [System.Serializable]
    public struct LevelStep {
        [Tooltip("명중 보정 (0.03 = +3%)")] public float hitBonus;
        [Tooltip("피해 보정 (0.05 = +5%)")] public float damageBonus;
    }

    [SerializeField] private LevelStep[] steps = {
        new LevelStep { hitBonus = 0.03f, damageBonus = 0.05f }, // Lv1
        new LevelStep { hitBonus = 0.06f, damageBonus = 0.10f }, // Lv2
        new LevelStep { hitBonus = 0.09f, damageBonus = 0.15f }, // Lv3
        new LevelStep { hitBonus = 0.12f, damageBonus = 0.20f }, // Lv4
        new LevelStep { hitBonus = 0.15f, damageBonus = 0.25f }, // Lv5
    };

    [Header("해금 게이트")]
    [SerializeField] private int twoHandUnlockLevel = 3;  // 양손 운용 해금 레벨
    [SerializeField] private int ultimateUnlockLevel = 5; // 무기 고유 필살기 해금 레벨

    public int MaxLevel => steps.Length;
    public int TwoHandUnlockLevel => twoHandUnlockLevel;
    public int UltimateUnlockLevel => ultimateUnlockLevel;

    // 레벨 0(미숙련)은 보정 없음. 1~MaxLevel은 표를 조회.
    public float HitBonus(int level) => level <= 0 ? 0f : Step(level).hitBonus;
    public float DamageBonus(int level) => level <= 0 ? 0f : Step(level).damageBonus;

    public bool IsTwoHandUnlocked(int level) => level >= twoHandUnlockLevel;
    public bool IsUltimateUnlocked(int level) => level >= ultimateUnlockLevel;

    private LevelStep Step(int level) => steps[Mathf.Clamp(level, 1, steps.Length) - 1];
}
