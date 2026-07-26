using System.Collections.Generic;
using UnityEngine;

// 플레이어가 지정할 수 있는 행동 목록의 단일 원천(single source of truth).
// 여러 시스템(패널 UI, 자율 행동 선택 등)이 이 카탈로그를 참조한다.
[CreateAssetMenu(fileName = "행동 카탈로그", menuName = "용병단/행동 카탈로그")]
public class ActionCatalog : ScriptableObject
{
    [SerializeField] private ActionDefinition[] actions;      // 플레이어가 지정 가능한 행동
    [SerializeField] private ActionDefinition defaultAction;  // 행동 미지정 시 자율 행동 (예: 휴식)

    public IReadOnlyList<ActionDefinition> Actions => actions;
    public ActionDefinition DefaultAction => defaultAction;
}
