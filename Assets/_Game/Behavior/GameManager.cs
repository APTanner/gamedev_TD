using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public LevelData LevelData;

    private int m_currentWave = 1;
    private bool m_bInWave = false;

    private float m_waveTime = 0;

    private List<EnemySpawner>[] m_spawners = new List<EnemySpawner>[0];

    [SerializeField] private TMP_Text waveText;

    public int WaveCount => m_spawners.Length;
    public float TimeLeftInWave => m_waveTime;

    // The index of the current Wave
    public static event Action<int> OnWaveStart;

    // The index of the next wave (or the last wave if there are no waves after this one)
    public static event Action<int> OnWaveEnd;

    private void Awake()
    {
        Instance = this;
    }

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

        PlayerMoney.Instance.AddMoney(LevelData.Money[0]);
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

    protected void FixedUpdate()
    {
        if (!m_bInWave)
        {
            return;
        }

        m_waveTime -= Time.fixedDeltaTime;

        SwarmerManager sm = SwarmerManager.Instance;
        // if everything has been killed
        if (sm.SwarmerCount == 0 || m_waveTime < 0)
        {
            EndWave();
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

        OnWaveStart?.Invoke(m_currentWave);

        foreach (EnemySpawner spawner in m_spawners[m_currentWave-1])
        {
            spawner.SpawnSwarmers();
        }

        m_waveTime = 1;
        m_bInWave = true;
    }

    public void EndWave()
    {
        if (!m_bInWave)
        {
            Debug.LogError("We are ending a wave that hasn't started yet. Something has gone wrong");
        }

        if (m_currentWave <= WaveCount && LevelData.Money.Length > m_currentWave)
        {
            PlayerMoney.Instance.AddMoney(LevelData.Money[m_currentWave]);
        }

        ++m_currentWave;

        OnWaveEnd?.Invoke(m_currentWave);
        SwarmerManager.Instance.Reset();
        UpdateWaveUI();


        m_bInWave = false;
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = $"{m_currentWave}";
        }
        else
        {
            Debug.LogWarning("Wave UI text not assigned in inspector.");
        }
    }
}
