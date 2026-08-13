using System.Collections.Generic;
using UnityEngine;

// 한 팀의 포메이션: 전열/후열 2열 × 3칸 = 6칸.
public class Formation
{
    public const int Columns = 3;
    private const int Rows = 2;

    // [row, column]
    private readonly Combatant[,] _slots = new Combatant[Rows, Columns];

    // 지정 칸에 배치. 이미 차 있으면 같은 열의 다른 열(전↔후) 또는 빈 칸으로 흘려보낸다.
    public bool Place(Combatant c, FormationRow row, int column) {
        column = Mathf.Clamp(column, 0, Columns - 1);
        if (TryPlaceExact(c, row, column)) return true;

        // 같은 열의 반대 행 시도
        FormationRow other = row == FormationRow.Front ? FormationRow.Back : FormationRow.Front;
        if (TryPlaceExact(c, other, column)) return true;

        // 아무 빈 칸
        for (int r = 0; r < Rows; r++)
            for (int col = 0; col < Columns; col++)
                if (TryPlaceExact(c, (FormationRow)r, col)) return true;

        return false; // 6칸 가득 참
    }

    private bool TryPlaceExact(Combatant c, FormationRow row, int column) {
        if (_slots[(int)row, column] != null) return false;
        _slots[(int)row, column] = c;
        c.SetPosition(row, column);
        return true;
    }

    public Combatant At(FormationRow row, int column) =>
        column < 0 || column >= Columns ? null : _slots[(int)row, column];

    public List<Combatant> RowAlive(FormationRow row) {
        var list = new List<Combatant>();
        for (int col = 0; col < Columns; col++) {
            var c = _slots[(int)row, col];
            if (c != null && c.IsAlive) list.Add(c);
        }
        return list;
    }

    public List<Combatant> AllAlive() {
        var list = new List<Combatant>();
        for (int r = 0; r < Rows; r++)
            for (int col = 0; col < Columns; col++) {
                var c = _slots[r, col];
                if (c != null && c.IsAlive) list.Add(c);
            }
        return list;
    }

    public bool HasAnyAlive() {
        for (int r = 0; r < Rows; r++)
            for (int col = 0; col < Columns; col++)
                if (_slots[r, col] != null && _slots[r, col].IsAlive) return true;
        return false;
    }

    public bool FrontRowHasAlive() => RowAlive(FormationRow.Front).Count > 0;

    // 넉백: 전열 유닛을 같은 열 후열로 밀어낸다(후열 칸이 비어 있을 때만). 성공 시 true.
    public bool Knockback(Combatant c) {
        if (c == null || c.Row != FormationRow.Front) return false;
        if (_slots[(int)FormationRow.Back, c.Column] != null) return false; // 뒤가 막힘
        _slots[(int)FormationRow.Front, c.Column] = null;
        _slots[(int)FormationRow.Back, c.Column] = c;
        c.SetPosition(FormationRow.Back, c.Column);
        return true;
    }
}
