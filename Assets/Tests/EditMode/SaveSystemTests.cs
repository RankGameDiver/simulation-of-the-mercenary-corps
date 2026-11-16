using System.IO;
using NUnit.Framework;
using UnityEngine;

// 진행 세이브 검증: 용병 저장·복원 왕복, 파일 쓰기·읽기, 잘못된 세이브 거부
public class SaveSystemTests
{
    private ActionDefinition _train;
    private ActionDefinition _rest;
    private ActionDefinition _locked;
    private ActionCatalog _actions;
    private WeaponDefinition _sword;
    private WeaponCatalog _weapons;
    private string _path;

    [SetUp] public void SetUp() {
        _train = Named(CombatTestFactory.Action("훈련", 1f), "Training");
        _rest = Named(CombatTestFactory.Action("휴식", 1f), "Rest");
        _locked = Named(CombatTestFactory.Action("잠김", 0f), "Locked");
        _actions = CombatTestFactory.ActionCatalog(_train, _rest, _locked);

        _sword = Named(CombatTestFactory.Sword(), "TrainingSword");
        _weapons = CombatTestFactory.WeaponCatalog(_sword);

        _path = Path.Combine(Path.GetTempPath(), $"mercenary-save-test-{System.Guid.NewGuid():N}.json");
    }

    [TearDown] public void TearDown() {
        if(File.Exists(_path)) File.Delete(_path);
        if(File.Exists(_path + ".tmp")) File.Delete(_path + ".tmp");
    }

    private static T Named<T>(T asset, string name) where T : Object {
        asset.name = name;
        return asset;
    }

    private DummyMercenaryData NewMercenary() =>
        new("테스트", _actions, _weapons, CombatTestFactory.Config());

    [Test] public void Mercenary_RoundTrip_Preserves_Stats() {
        var original = NewMercenary();
        var restored = DummyMercenaryData.FromSave(original.ToSaveData(), _actions, _weapons);

        Assert.AreEqual(original.Id, restored.Id);
        Assert.AreEqual(original.Name, restored.Name);
        Assert.AreEqual(original.Stamina, restored.Stamina);
        Assert.AreEqual(original.Strength, restored.Strength);
        Assert.AreEqual(original.Agility, restored.Agility);
        Assert.AreEqual(original.Health, restored.Health);
        Assert.AreEqual(original.Intelligence, restored.Intelligence);
        Assert.AreEqual(original.Charming, restored.Charming);
        Assert.AreEqual(original.CurrentHp, restored.CurrentHp);
    }

    [Test] public void Mercenary_RoundTrip_Preserves_Unlocked_Actions() {
        var restored = DummyMercenaryData.FromSave(NewMercenary().ToSaveData(), _actions, _weapons);

        Assert.IsTrue(restored.IsActionUnlocked(_train));
        Assert.IsTrue(restored.IsActionUnlocked(_rest));
        Assert.IsFalse(restored.IsActionUnlocked(_locked));
    }

    [Test] public void Mercenary_RoundTrip_Preserves_Loadout() {
        var original = NewMercenary();
        original.Loadout.SetGrip(GripMode.OneHandShield);
        original.Loadout.SetLevel(Proficiency.ShieldArts, 2);

        var restored = DummyMercenaryData.FromSave(original.ToSaveData(), _actions, _weapons);

        Assert.AreSame(_sword, restored.Loadout.Weapon);
        Assert.AreEqual(GripMode.OneHandShield, restored.Loadout.Grip);
        Assert.AreEqual(original.Loadout.WeaponLevel, restored.Loadout.WeaponLevel);
        Assert.AreEqual(2, restored.Loadout.ShieldLevel);
    }

    [Test] public void Restored_Mercenary_Has_No_Routine() {
        var original = NewMercenary();
        original.AssignAction(_train);

        var restored = DummyMercenaryData.FromSave(original.ToSaveData(), _actions, _weapons);

        Assert.IsNull(restored.CurrentAction);
    }

    [Test] public void Unknown_Assets_Are_Skipped_On_Restore() {
        var data = NewMercenary().ToSaveData();
        data.unlockedActions.Add("Removed");
        data.weapon = "RemovedWeapon";

        var restored = DummyMercenaryData.FromSave(data, _actions, _weapons);

        Assert.AreEqual(2, restored.UnlockedActions.Count);
        Assert.IsNull(restored.Loadout);
    }

    [Test] public void File_RoundTrip_Preserves_Turn_And_Mercenary() {
        var merc = NewMercenary();
        Assert.IsTrue(SaveSystem.Write(_path, new SaveData { turn = 11, mercenary = merc.ToSaveData() }));

        Assert.IsTrue(SaveSystem.TryRead(_path, out var loaded));
        Assert.AreEqual(11, loaded.turn);
        Assert.AreEqual(merc.Name, loaded.mercenary.name);
        Assert.AreEqual(SaveData.CurrentVersion, loaded.version);
        Assert.IsFalse(string.IsNullOrEmpty(loaded.savedAtUtc));
    }

    [Test] public void Write_Overwrites_Existing_Save() {
        SaveSystem.Write(_path, new SaveData { turn = 1, mercenary = NewMercenary().ToSaveData() });
        SaveSystem.Write(_path, new SaveData { turn = 5, mercenary = NewMercenary().ToSaveData() });

        Assert.IsTrue(SaveSystem.TryRead(_path, out var loaded));
        Assert.AreEqual(5, loaded.turn);
        Assert.IsFalse(File.Exists(_path + ".tmp"));
    }

    [Test] public void TryRead_Missing_File_Returns_False() {
        Assert.IsFalse(SaveSystem.TryRead(_path, out var loaded));
        Assert.IsNull(loaded);
    }

    [Test] public void TryRead_Corrupt_File_Returns_False() {
        File.WriteAllText(_path, "{ 이건 JSON이 아니다");
        Assert.IsFalse(SaveSystem.TryRead(_path, out _));
    }

    [Test] public void TryRead_Other_Version_Returns_False() {
        var json = JsonUtility.ToJson(new SaveData { version = SaveData.CurrentVersion + 1, mercenary = NewMercenary().ToSaveData() });
        File.WriteAllText(_path, json);
        Assert.IsFalse(SaveSystem.TryRead(_path, out _));
    }

    [Test] public void TryRead_Without_Mercenary_Returns_False() {
        File.WriteAllText(_path, $"{{\"version\":{SaveData.CurrentVersion},\"turn\":3}}");
        Assert.IsFalse(SaveSystem.TryRead(_path, out _));
    }
}
