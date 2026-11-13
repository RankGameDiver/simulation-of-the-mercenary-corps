using NUnit.Framework;

// 일과 지정 규칙 검증 (기획서 4.1 [확정])
// - 일과는 직접 고른다. 미지정 상태에서 실행해도 자율 행동으로 채워지지 않는다
// - 해금되지 않은 일과는 지정할 수 없다
// - 새 턴이 시작되면 일과가 비워진다
public class MercenaryRoutineTests
{
    private ActionDefinition _train;
    private ActionDefinition _rest;
    private ActionDefinition _locked;
    private DummyMercenaryData _merc;

    [SetUp] public void SetUp() {
        _train = CombatTestFactory.Action("훈련", 1f);
        _rest = CombatTestFactory.Action("휴식", 1f);
        _locked = CombatTestFactory.Action("잠김", 0f);
        _merc = new DummyMercenaryData("테스트", CombatTestFactory.ActionCatalog(_train, _rest, _locked));
    }

    [Test] public void NewMercenary_Has_No_Routine() {
        Assert.IsNull(_merc.CurrentAction);
    }

    [Test] public void Execute_Without_Routine_Does_Not_Fill_Default() {
        _merc.OnTurnExecute();
        Assert.IsNull(_merc.CurrentAction);
    }

    [Test] public void Routine_Can_Be_Changed_Before_Turn_Ends() {
        _merc.AssignAction(_train);
        _merc.AssignAction(_rest);
        Assert.AreSame(_rest, _merc.CurrentAction);
    }

    [Test] public void Locked_Routine_Cannot_Be_Assigned() {
        Assert.IsFalse(_merc.IsActionUnlocked(_locked));
        _merc.AssignAction(_train);
        _merc.AssignAction(_locked);
        Assert.AreSame(_train, _merc.CurrentAction);
    }

    [Test] public void TurnStart_Clears_Routine() {
        _merc.AssignAction(_train);
        _merc.OnTurnStart();
        Assert.IsNull(_merc.CurrentAction);
    }

    [Test] public void NewMercenary_Starts_At_Full_Hp() {
        Assert.AreEqual(_merc.Health * MercenaryData.HpPerHealth, _merc.MaxHp);
        Assert.AreEqual(_merc.MaxHp, _merc.CurrentHp);
    }
}
