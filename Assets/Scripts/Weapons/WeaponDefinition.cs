using UnityEngine;

// 무기 하나(검/창/둔기)의 정체성을 정의하는 애셋. ActionDefinition과 같은 패턴.
// 무기 종류가 늘어나면 이 애셋을 하나씩 추가하고 WeaponCatalog에 등록한다.
[CreateAssetMenu(fileName = "새 무기", menuName = "용병단/무기 정의")]
public class WeaponDefinition : ScriptableObject
{
    [SerializeField] private string displayName;                 // 표시 이름 (예: "장검")
    [SerializeField, TextArea] private string description;       // 향후 툴팁용 설명

    [Header("분류")]
    [SerializeField] private WeaponClass weaponClass;            // 검 / 창 / 둔기
    [SerializeField] private Proficiency governingProficiency;   // 이 무기를 지배하는 숙련

    [Header("주 스탯 스케일링 (유효공격 = 힘*힘가중 + 민첩*민첩가중)")]
    // 검=(0,1) 민첩, 창=(0.5,0.5) (힘+민첩)/2, 둔기=(1,0) 힘
    [SerializeField, Range(0f, 1f)] private float strengthWeight = 0f;
    [SerializeField, Range(0f, 1f)] private float agilityWeight = 1f;

    [Header("사거리 (칸)")]
    [SerializeField] private int oneHandRange = 1;              // 한손 운용 사거리 (창=2)
    [SerializeField] private int twoHandRange = 1;             // 양손 운용 사거리 (양손창=3)

    [Header("양손 전용 효과")]
    [SerializeField] private TwoHandEffect twoHandEffect = TwoHandEffect.None;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? weaponClass.Korean() : displayName;
    public string Description => description;
    public WeaponClass WeaponClass => weaponClass;
    public Proficiency GoverningProficiency => governingProficiency;
    public float StrengthWeight => strengthWeight;
    public float AgilityWeight => agilityWeight;
    public int OneHandRange => oneHandRange;
    public int TwoHandRange => twoHandRange;
    public TwoHandEffect TwoHandEffect => twoHandEffect;

    // 운용에 따른 실제 사거리
    public int RangeFor(GripMode grip) => grip == GripMode.TwoHand ? twoHandRange : oneHandRange;
}
