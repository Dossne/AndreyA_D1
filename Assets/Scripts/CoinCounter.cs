using UnityEngine;
using UnityEngine.UI;

public class CoinCounter : MonoBehaviour
{
    private const int StartingCoins = 0;

    public static CoinCounter Instance { get; private set; }

    private Text text;
    private int coins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        text = GetComponent<Text>();
        coins = StartingCoins;
        UpdateText();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        coins += amount;
        UpdateText();
    }

    private void UpdateText()
    {
        if (text != null)
        {
            text.text = $"Coins: {coins}";
        }
    }
}