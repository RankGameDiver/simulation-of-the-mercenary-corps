using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private int dummyMercenaryCount = 5;

    void Awake() {
        // 더미 용병 생성 후 TurnManager에 등록
        var names = new[] { "카일", "레나", "미아", "루드거" };
        for (int i = 0; i < Mathf.Min(dummyMercenaryCount, names.Length); i++) {
            TurnManager.Instance.RegisterMercenary(new DummyMercenaryData(names[i]));
        }
    }
}