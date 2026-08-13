using System.Collections.Generic;
using UnityEngine;

// 초기 숙련도 레벨을 가중치에 따라 뽑는 헬퍼 (순수 규칙, 테스트 대상).
// 단일 무기 특화: 용병마다 무기 하나만 특화하고 그 레벨을 이 가중 롤로 정한다.
public static class WeaponRoll
{
    // weights[i] → Lv.(i+1)의 상대 가중치. 높은 레벨일수록 작은 가중치를 주면 고레벨이 희소해진다.
    public static int WeightedLevel(IReadOnlyList<int> weights) {
        if (weights == null || weights.Count == 0) return 1;

        int total = 0;
        foreach (var w in weights) total += Mathf.Max(0, w);
        if (total <= 0) return 1;

        int roll = Random.Range(0, total);
        int acc = 0;
        for (int i = 0; i < weights.Count; i++) {
            acc += Mathf.Max(0, weights[i]);
            if (roll < acc) return i + 1; // 인덱스 0 → Lv.1
        }
        return weights.Count;
    }
}
