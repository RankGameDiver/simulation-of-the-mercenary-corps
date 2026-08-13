using System.Collections.Generic;
using UnityEngine;

// 게임에 존재하는 무기 목록의 단일 원천(single source of truth).
// ActionCatalog와 같은 패턴. 용병 생성 시 이 목록에서 무기를 뽑아 특화시킨다.
[CreateAssetMenu(fileName = "무기 카탈로그", menuName = "용병단/무기 카탈로그")]
public class WeaponCatalog : ScriptableObject
{
    [SerializeField] private WeaponDefinition[] weapons; // 획득 가능한 무기 전체

    public IReadOnlyList<WeaponDefinition> Weapons => weapons;

    // 목록에서 무작위 무기 하나 (없으면 null)
    public WeaponDefinition RandomWeapon() {
        if (weapons == null || weapons.Length == 0) return null;
        return weapons[Random.Range(0, weapons.Length)];
    }
}
