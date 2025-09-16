using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    [SerializeField] private int minCoinPerKill = 1;
    [SerializeField] private int maxCoinPerKill = 5;

    public void GainCoins()
    {
        var coinAmount = Random.Range(minCoinPerKill, maxCoinPerKill);
        GameManager.Instance.AddCoins(coinAmount);
    }
}