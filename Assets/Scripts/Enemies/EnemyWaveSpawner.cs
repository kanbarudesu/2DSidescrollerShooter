using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using ImprovedTimers;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string spawnConfigLabel = "SpawnConfigs";
    [SerializeField] private float waveInterval = 5f;
    [SerializeField] private Collider2D groundArea;

    private Camera mainCamera;
    private CountdownTimer waveClearTimer;

    private readonly List<SpawnConfig> allConfigs = new List<SpawnConfig>();
    private List<WaveData> mergedWaves = new List<WaveData>();

    public int CurrentWave = 1;
    public int EnemyCount { get; private set; }
    public List<GameObject> ActiveEnemies { get; private set; } = new List<GameObject>();

    public UnityEvent OnWaveCleared;

    private IEnumerator Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        waveClearTimer = new CountdownTimer(3);
        waveClearTimer.OnTimerStop += () =>
        {
            CurrentWave++;
            OnWaveCleared?.Invoke();
        };

        var handle = Addressables.LoadAssetsAsync<SpawnConfig>(spawnConfigLabel, config =>
        {
            allConfigs.Add(config);
        });
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var config in allConfigs)
                mergedWaves.AddRange(config.Waves);
        }
        else
        {
            Debug.LogError("Failed to load SpawnConfigs!");
        }
    }

    private void Update()
    {
        if (waveClearTimer.IsRunning && EnemyCount > 0)
            waveClearTimer.Stop();
    }

    public void SpawnRandomWave(int waveAmount = 1)
    {
        if (mergedWaves.Count == 0)
        {
            Debug.LogWarning("No waves available!");
            return;
        }
        StartCoroutine(SpawnIntervalRoutine(waveAmount));
    }

    private IEnumerator SpawnIntervalRoutine(int waveAmount)
    {
        for (int w = 0; w < waveAmount; w++)
        {
            int waveIndex = Random.Range(0, mergedWaves.Count);
            yield return StartCoroutine(SpawnWave(mergedWaves[waveIndex]));
            yield return new WaitForSeconds(waveInterval);
        }
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        EnemyCount += wave.SpawnCount;
        for (int i = 0; i < wave.SpawnCount; i++)
        {
            yield return SpawnEnemy(wave);
            yield return new WaitForSeconds(wave.SpawnInterval);
        }
    }

    private IEnumerator SpawnEnemy(WaveData wave)
    {
        if (wave.EnemyAddresses == null || wave.EnemyAddresses.Length == 0)
            yield break;

        AssetReferenceGameObject enemyRef =
            wave.EnemyAddresses[Random.Range(0, wave.EnemyAddresses.Length)];
        Vector2 spawnPos = wave.SpawnRandomly ? GetRandomSpawnPosition() : GetEdgeSpawnPosition();

        var handle = enemyRef.InstantiateAsync(spawnPos, Quaternion.identity);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject enemy = handle.Result;
            ActiveEnemies.Add(enemy);

            if (enemy.TryGetComponent(out Health health))
            {
                health.OnDie.AddListener((deadEnemy) => OnEnemyDied(deadEnemy, enemyRef));
            }
        }
        else
        {
            Debug.LogError($"Failed to spawn enemy: {enemyRef.RuntimeKey}");
        }
    }

    private void OnEnemyDied(GameObject enemy, AssetReferenceGameObject enemyRef)
    {
        EnemyCount--;
        ActiveEnemies.Remove(enemy);
        Addressables.ReleaseInstance(enemy);

        if (EnemyCount <= 0)
            waveClearTimer.Start();
    }

    private Vector2 GetEdgeSpawnPosition()
    {
        Vector3 camPos = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;

        float x = (Random.value < 0.5f)
            ? camPos.x - camWidth / 2f - 1f
            : camPos.x + camWidth / 2f + 1f;

        Bounds groundBounds = groundArea.bounds;
        float y = Random.Range(groundBounds.min.y, groundBounds.max.y);

        return new Vector2(x, Mathf.Clamp(y, groundBounds.min.y, groundBounds.max.y));
    }

    private Vector2 GetRandomSpawnPosition()
    {
        BoxCollider2D box = groundArea as BoxCollider2D;
        Vector2 size = box.size;
        Vector2 center = box.offset;
        Vector2 randomPoint = new Vector2(
            Random.Range(-size.x / 2f, size.x / 2f),
            Random.Range(-size.y / 2f, size.y / 2f)
        );
        return box.transform.TransformPoint(center + randomPoint);
    }
}
