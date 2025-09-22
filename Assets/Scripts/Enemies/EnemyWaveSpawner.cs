using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using ImprovedTimers;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyWaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct WaveData
    {
        [Tooltip("Addressable enemy prefab references")]
        public AssetReferenceGameObject[] EnemyAddresses;
        public int SpawnCount;
        public float SpawnInterval;
        public bool SpawnRandomly;
    }

    [SerializeField] private WaveData[] waves;
    [SerializeField] private float waveInterval = 5f;
    [SerializeField] private Collider2D groundArea;

    private Camera mainCamera;
    private CountdownTimer waveCleartimer;

    public int CurrentWave = 1;
    public int EnemyCount { get; private set; }
    public List<GameObject> ActiveEnemies { get; private set; } = new List<GameObject>();

    public UnityEvent OnWaveCleared;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        waveCleartimer = new CountdownTimer(3);
        waveCleartimer.OnTimerStop += () =>
        {
            CurrentWave++;
            OnWaveCleared?.Invoke();
        };
    }

    private void Update()
    {
        if (waveCleartimer.IsRunning && EnemyCount > 0)
            waveCleartimer.Stop();
    }

    public void SpawnRandomWave(int waveAmount = 1)
    {
        StartCoroutine(SpawnIntervalRoutine(waveAmount));
    }

    private IEnumerator SpawnIntervalRoutine(int waveAmount = 1)
    {
        for (int w = 0; w < waveAmount; w++)
        {
            int waveIndex = Random.Range(0, waves.Length);

            yield return StartCoroutine(SpawnWave(waves[waveIndex]));
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
        if (wave.EnemyAddresses == null || wave.EnemyAddresses.Length == 0) yield break;

        AssetReferenceGameObject enemyRef =
            wave.EnemyAddresses[Random.Range(0, wave.EnemyAddresses.Length)];
        Vector2 spawnPos = wave.SpawnRandomly ? GetRandomSpawnPosition() : GetSpawnPosition();

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
            Debug.LogError($"Failed to load enemy: {enemyRef.RuntimeKey}");
        }
    }

    private void OnEnemyDied(GameObject enemy, AssetReferenceGameObject enemyRef)
    {
        EnemyCount--;
        ActiveEnemies.Remove(enemy);

        Addressables.ReleaseInstance(enemy);

        if (EnemyCount <= 0)
        {
            waveCleartimer.Start();
        }
    }

    private Vector2 GetSpawnPosition()
    {
        Vector3 camPos = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;

        float x = (Random.value < 0.5f) ? camPos.x - camWidth / 2f - 1f : camPos.x + camWidth / 2f + 1f;

        Bounds groundBounds = groundArea.bounds;
        float yMin = groundBounds.min.y;
        float yMax = groundBounds.max.y;
        float y = Random.Range(yMin, yMax);

        return new Vector2(x, Mathf.Clamp(y, yMin, yMax));
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
