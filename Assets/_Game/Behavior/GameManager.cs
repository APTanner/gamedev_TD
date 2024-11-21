using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LevelData LevelData;

    private int m_currentWave = 1;
    private bool m_bInWave = false;

    private List<EnemySpawner>[] m_spawners = new List<EnemySpawner>[0];

    public void Start()
    {
        if (LevelData == null)
        {
            GridManager.Instance.InitializeEmptyLevel();
            return;
        }

        GridManager.Instance.InitializeLevelGridData(LevelData);
        GridManager.Instance.InitializeLevelGridData(LevelData);
        InitializeSpawners();
    }

    private void InitializeSpawners()
    {
        if (LevelData.Spawners == null)
        {
            Debug.LogError("This level has no spawners");
            return;
        }

        EnemySpawner[] invalidSpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        foreach (EnemySpawner spawner in invalidSpawners)
        {
            Destroy(spawner.gameObject);
        }

        int maxWave = LevelData.Spawners.Max(x => x.WaveNum);
        m_spawners = Enumerable.Range(0, maxWave)
                               .Select(_ => new List<EnemySpawner>())
                               .ToArray();

        foreach (SpawnerData spawner in LevelData.Spawners)
        {
            var spawnerGo = Instantiate(
                PrefabManager.Instance.EnemySpawnerPrefab,
                spawner.Position,
                spawner.Rotation);
            m_spawners[spawner.WaveNum-1].Add(spawnerGo);
        }
    }

    public void BeginWave()
    {
        if (m_bInWave)
        {
            Debug.LogError("Tried to start a wave while the previous wave was still ongoing");
        }
        if (m_currentWave > m_spawners.Length)
        {
            Debug.LogError("Trying to start another wave after this level has ended");
            return;
        }
        if (m_spawners[m_currentWave-1].Count == 0)
        {
            Debug.LogError($"Wave {m_currentWave} has no spawners. Fix this");
            return;
        }

        foreach (EnemySpawner spawner in m_spawners[m_currentWave-1])
        {
            spawner.SpawnSwarmers();
        }

        m_bInWave = true;

        // TODO - for now, we will just immediately go to the next wave
        // Ideally, we would wait until all the swarmers are dead
        EndWave();
    }

    public void EndWave()
    {
        if (!m_bInWave)
        {
            Debug.LogError("We are ending a wave that hasn't started yet. Something has gone wrong");
        }
        m_bInWave = false;
        ++m_currentWave;
    }
}
