using System.Collections.Generic;
using UnityEngine;

// 플레이어가 지정할 수 있는 일과(행동) 목록의 단일 원천(single source of truth).
// 여러 시스템(일과 패널, 키보드 단축키 등)이 이 카탈로그를 참조한다.
// 일과는 플레이어가 직접 고르며, 미지정 시 대신 수행하는 자율 행동은 없다.
[CreateAssetMenu(fileName = "ActionCatalog", menuName = "용병단/행동 카탈로그")]
public class ActionCatalog : ScriptableObject
{
    [SerializeField] private ActionDefinition[] actions; // 표시 순서 = 단축키 순서(1, 2, 3 …)

    public IReadOnlyList<ActionDefinition> Actions => actions;
}
