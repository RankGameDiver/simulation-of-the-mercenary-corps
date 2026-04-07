using UnityEngine;
using TMPro;

public class ResourceBar : MonoBehaviour
{
    [Header("골드")]
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("보급품")]
    [SerializeField] private TextMeshProUGUI supplyText;

    [Header("영향력")]
    [SerializeField] private TextMeshProUGUI influenceText;

    // 현재 자원량 (나중에 ResourceManager 생기면 구독 방식으로 교체)
    private int _gold = 1000;
    private int _supply = 50;
    private int _influence = 10;

    void Start() => Render();

    // 외부에서 자원 변경 시 호출
    public void SetResources(int gold, int supply, int influence)
    {
        _gold = gold;
        _supply = supply;
        _influence = influence;
        Render();
    }

    public void AddGold(int amount) { 
        _gold += amount;
        Render(); 
    }
    
    public void AddSupply(int amount)   { 
        _supply += amount;
        Render(); 
    }
    
    public void AddInfluence(int amount) { 
        _influence += amount;
        Render(); 
    }

    private void Render()
    {
        goldText.text      = $"💰 {_gold:N0}";
        supplyText.text    = $"🎒 {_supply:N0}";
        influenceText.text = $"⭐ {_influence:N0}";
    }
}