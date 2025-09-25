using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "SpawnConfig", menuName = "SpawnConfig", order = 0)]
public class SpawnConfig : ScriptableObject
{
    public WaveData[] Waves;
}

[System.Serializable]
public struct WaveData
{
    [Tooltip("Addressable enemy prefab references")]
    public AssetReferenceGameObject[] EnemyAddresses;
    public int SpawnCount;
    public float SpawnInterval;
    public bool SpawnRandomly;
}