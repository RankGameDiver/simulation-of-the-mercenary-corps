using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MercenaryListPanel : PanelBase
{
    [Header("참조")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject mercenaryItemPrefab;

    // id -> 아이템 매핑 (RefreshItem 빠른 접근용)
    private readonly Dictionary<string, MercenaryListItem> _itemMap = new();

    public void Refresh(IReadOnlyList<IMercenary> mercenaries) {
        // 기존 아이템 전부 제거
        foreach(Transform child in itemContainer) {
            Destroy(child.gameObject);
        }

        _itemMap.Clear();

        foreach(var m in mercenaries) {
            CreateItem(m);
        }
    }

    // 특정 용병 상태만 갱신 (전체 재생성 방지)
    public void RefreshItem(IMercenary m) {
        if(_itemMap.TryGetValue(m.Id, out var item)) {
            item.Bind(m);
        }
    }

    private void CreateItem(IMercenary m) {
        var go = Instantiate(mercenaryItemPrefab, itemContainer);
        var item = go.GetComponent<MercenaryListItem>();
        item.Bind(m);
        _itemMap[m.Id] = item;
    }
}