using ImprovedTimers;
using Kanbarudesu.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("References")]
    [SerializeField] private EnemyWaveSpawner spawner;
    [SerializeField] private Health playerHealth;
    [SerializeField] private Transform zoneArea;

    [Header("UI")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemyCountText;
    [SerializeField] private TMP_Text leftEnemyText;
    [SerializeField] private TMP_Text rightEnemyText;
    [SerializeField] private GameObject leftEnemyIndicator;
    [SerializeField] private GameObject rightEnemyIndicator;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("GameoverUI")]
    [SerializeField] private TMP_Text highestWaveText;
    [SerializeField] private TMP_Text currentWaveText;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject gameoverPanel;

    private int coins;
    private FrequencyTimer timer;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        coins = 0;
        coinsText.text = coins.ToString();

        timer = new FrequencyTimer(10);
        timer.OnTick += UpdateUI;
        timer.Start();

        pauseButton.onClick.AddListener(() => Time.timeScale = 0);
        resumeButton.onClick.AddListener(() => Time.timeScale = 1);
        quitButton.onClick.AddListener(() => Application.Quit());
        mainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(0);
        });

        spawner.OnWaveCleared.AddListener(EnableZone);
        playerHealth.OnDie.AddListener(OnPlayerDied);
        restartButton.onClick.AddListener(RestartGame);
    }

    private void OnPlayerDied(GameObject obj)
    {
        Time.timeScale = 0;

        int highestWave = PlayerPrefs.GetInt("highestWave", spawner.CurrentWave);
        if (spawner.CurrentWave >= highestWave)
            PlayerPrefs.SetInt("highestWave", spawner.CurrentWave);

        highestWaveText.text = $"Highest Wave Cleared: {highestWave}";
        currentWaveText.text = $"Current Wave: {spawner.CurrentWave}";

        gameoverPanel.SetActive(true);
    }

    private void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Gameplay");
    }

    private void EnableZone()
    {
        var worldPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        zoneArea.transform.position = new Vector3(worldPos.x, 0f, 0f);
        zoneArea.gameObject.SetActive(true);
    }

    private void UpdateUI()
    {
        waveText.text = $"Wave {spawner.CurrentWave}";
        enemyCountText.text = $"{spawner.EnemyCount} Enemy Left";

        CountEnemiesBySide();
    }

    private void CountEnemiesBySide()
    {
        if (spawner == null) return;

        Vector3 leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f));
        Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, 0f));

        int leftCount = 0;
        int rightCount = 0;

        List<GameObject> enemies = spawner.ActiveEnemies;
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            float x = enemy.transform.position.x;

            if (x < leftEdge.x)
                leftCount++;
            else if (x > rightEdge.x)
                rightCount++;
        }

        leftEnemyText.text = $"{leftCount}";
        rightEnemyText.text = $"{rightCount}";

        leftEnemyIndicator.SetActive(leftCount > 0);
        rightEnemyIndicator.SetActive(rightCount > 0);
    }

    [ContextMenu("Start Wave")]
    public void StartWave()
    {
        int waveAmount = 1 + ((spawner.CurrentWave - 1) / 3);
        spawner.SpawnRandomWave(waveAmount);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        coinsText.text = coins.ToString();
    }

    public bool CanSpendCoins(int amount)
    {
        if (amount > coins)
        {
            return false;
        }

        coins -= amount;
        coinsText.text = coins.ToString();
        return true;
    }
}
