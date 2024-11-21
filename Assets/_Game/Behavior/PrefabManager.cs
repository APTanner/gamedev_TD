using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }

    public SwarmerController SwarmerPrefab;
    public EnemySpawner EnemySpawnerPrefab;
    public Wall ObstaclePrefab;
    public HQ HQPrefab;

    public void Awake()
    {
        Instance = this;
    }
}
