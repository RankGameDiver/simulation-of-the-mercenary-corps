using UnityEngine;
using UnityEngine.Serialization;

// 용병이 수행할 수 있는 행동 하나를 정의하는 애셋.
// 행동 종류가 늘어나면 이 애셋을 하나씩 추가한다.
[CreateAssetMenu(fileName = "새 행동", menuName = "용병단/행동 정의")]
public class ActionDefinition : ScriptableObject
{
    [SerializeField] private string displayName; // 버튼/표시에 쓰는 이름 (예: "훈련")
    [SerializeField, TextArea] private string description; // 향후 툴팁용 설명

    [Header("해금")]
    [SerializeField, Range(0f, 1f), FormerlySerializedAs("randomUnlockChance")]
    private float unlockChance = 0.5f; // 용병 생성 시 해금 확률 (1=항상 해금, 0=해금 안 됨)
    [SerializeField] private bool hideWhenLocked = false; // 미해금 시 true=버튼 숨김, false=비활성(회색) 표시

    public string DisplayName => displayName;
    public string Description => description;
    public float UnlockChance => unlockChance;
    public bool HideWhenLocked => hideWhenLocked;
}