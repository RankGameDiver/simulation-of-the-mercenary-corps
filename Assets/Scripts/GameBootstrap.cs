using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private int dummyMercenaryCount = 5;

    void Start() {
        // 더미 용병 생성 후 TurnManager에 등록
        var names = new[] { "카일", "레나", "미아", "루드거" };
        for (int i = 0; i < Mathf.Min(dummyMercenaryCount, names.Length); i++) {
            var m = new DummyMercenaryData(names[i]);
            TurnManager.Instance.RegisterMercenary(m);
            Debug.Log($"[용병 생성] {m.Name} (Id: {m.Id})\n" +
                      $"  성격: {m.PersonalityType}\n" +
                      $"  힘: {m.Strength} | 민첩: {m.Agility} | 지능: {m.Intelligence} | 매력: {m.Charming}");
        }
    }
}