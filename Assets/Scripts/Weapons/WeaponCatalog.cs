using System.Collections.Generic;
using UnityEngine;

// 게임에 존재하는 무기 목록의 단일 원천(single source of truth).
// ActionCatalog와 같은 패턴. 용병 생성 시 이 목록에서 무기를 뽑아 특화시킨다.
[CreateAssetMenu(fileName = "WeaponCatalog", menuName = "용병단/무기 카탈로그")]
public class WeaponCatalog : ScriptableObject
{
    [SerializeField] private WeaponDefinition[] weapons; // 획득 가능한 무기 전체

    public IReadOnlyList<WeaponDefinition> Weapons => weapons;

    // 목록에서 무작위 무기 하나 (없으면 null)
    public WeaponDefinition RandomWeapon() {
        if (weapons == null || weapons.Length == 0) return null;
        return weapons[Random.Range(0, weapons.Length)];
    }

    // 애셋 이름으로 찾는다 (세이브 복원용). 없으면 null
    public WeaponDefinition FindByName(string assetName) {
        if(weapons == null || string.IsNullOrEmpty(assetName)) return null;
        foreach(var weapon in weapons) {
            if(weapon != null && weapon.name == assetName) return weapon;
        }
        return null;
    }
}
