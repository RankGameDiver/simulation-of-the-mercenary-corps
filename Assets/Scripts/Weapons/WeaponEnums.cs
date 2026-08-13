// 근접무기 시스템의 핵심 열거형과 한글 라벨 헬퍼.
// 식별자는 영어(프로젝트 관례), 주석·표시 문자열은 한국어.

// 무기 3종
public enum WeaponClass {
    Sword, // 검  — 명중·연타·회피 (민첩 연동)
    Spear, // 창  — 사거리·선공 견제 (힘+민첩 연동)
    Blunt, // 둔기 — 방어 관통·파괴 (힘 연동)
}

// 숙련 4종
public enum Proficiency {
    Swordsmanship, // 검술
    Spearmanship,  // 창술
    BluntArts,     // 둔기술
    ShieldArts,    // 방패술
}

// 운용 방식 3종
public enum GripMode {
    OneHand,       // 한손 단독 (오프핸드 유격/정찰형)
    OneHandShield, // 한손+방패 (전열 고정형)
    TwoHand,       // 양손 (하이리스크 하이리턴)
}

// 포메이션 열
public enum FormationRow {
    Front, // 전열
    Back,  // 후열
}

// 양손 전용 강화 효과 (한손 특성을 극단화)
public enum TwoHandEffect {
    None,                // 없음
    AoeRow,              // 양손검: 다단 히트 → 1열 전체 광역
    Pierce,              // 양손창: 사거리 3칸, 직선 2체 관통
    ArmorBreakKnockback, // 양손둔기: 방어 완전 무시 + 넉백/기절
}

// 상성 판정 결과
public enum MatchupResult {
    Advantage,    // 유리
    Neutral,      // 중립
    Disadvantage, // 불리
}

// UI·로그용 한글 라벨
public static class WeaponText {
    public static string Korean(this WeaponClass c) => c switch {
        WeaponClass.Sword => "검",
        WeaponClass.Spear => "창",
        WeaponClass.Blunt => "둔기",
        _ => c.ToString(),
    };

    public static string Korean(this Proficiency p) => p switch {
        Proficiency.Swordsmanship => "검술",
        Proficiency.Spearmanship => "창술",
        Proficiency.BluntArts => "둔기술",
        Proficiency.ShieldArts => "방패술",
        _ => p.ToString(),
    };

    public static string Korean(this GripMode g) => g switch {
        GripMode.OneHand => "한손 단독",
        GripMode.OneHandShield => "한손+방패",
        GripMode.TwoHand => "양손",
        _ => g.ToString(),
    };

    public static string Korean(this FormationRow r) => r switch {
        FormationRow.Front => "전열",
        FormationRow.Back => "후열",
        _ => r.ToString(),
    };
}
