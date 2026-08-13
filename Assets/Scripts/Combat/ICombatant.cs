// 전투에 참여하는 유닛(아군 용병 또는 적)이 전투 시스템에 노출하는 계약.
// 전투 로직은 구체 타입이 아니라 이 인터페이스에만 의존한다.
public interface ICombatant
{
    string Name { get; }
    bool IsPlayer { get; }        // true=아군 용병, false=적

    int MaxHP { get; }
    int CurrentHP { get; }
    bool IsAlive { get; }

    int Strength { get; }
    int Agility { get; }
    WeaponLoadout Loadout { get; } // 무기·운용·숙련도 (없으면 null = 맨손)

    FormationRow Row { get; }
    int Column { get; }

    void SetPosition(FormationRow row, int column);
    void TakeDamage(float amount);
}
