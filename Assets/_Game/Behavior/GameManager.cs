using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public LevelData LevelData;

    public static GameState State => s_state;

    private int m_currentWave = 1;

    private static GameState s_state = GameState.None;

    private float m_waveTime = 0;

    private List<EnemySpawner>[] m_spawners = new List<EnemySpawner>[0];

    [SerializeField] private TMP_Text waveText;

    public int WaveCount => m_spawners.Length;
    public float TimeLeftInWave => m_waveTime;

    public Vector2Int HQCoords => LevelData.HQCoordinates;
    public Vector3 GetHQPosition()
    {
        GridManager gm = GridManager.Instance;
        Vector3 offsetToCenter = MathFunctions.ToVector3(((Vector2)HQ.BuildingSize) / 2f);
        return gm.GetCellCenter(HQCoords) +
            offsetToCenter * Defines.BuildingGridCellSize;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void SetupLevel()
    {
        if (LevelData == null)
        {
            GridManager.Instance.InitializeEmptyLevel();
            return;
        }

        m_currentWave = 1;
        m_waveTime = 0;
        s_state = GameState.Building;

        BuildingManager.Instance.ClearAllBuildings();
        GridManager.Instance.InitializeLevelGridData(LevelData);
        InitializeSpawners();

        PlayerMoney pm = PlayerMoney.Instance;
        pm.ResetMoney();
        pm.AddMoney(LevelData.Money[0]);
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
            spawnerGo.enemyNum = spawner.EnemyCount;
            spawnerGo.waveNum = spawner.WaveNum;
        }
    }

    protected void FixedUpdate()
    {
        if (s_state != GameState.InWave)
        {
            return;
        }

        m_waveTime -= Time.fixedDeltaTime;

        if (Switchboard.HQHealth <= 0)
        {
            Switchboard.Lose();
            s_state = GameState.LevelOver;
            EndWave();
        }

        SwarmerManager sm = SwarmerManager.Instance;
        // if everything has been killed
        if (sm.SwarmerCount == 0 || m_waveTime < 0)
        {
            EndWave();
        }
    }

    public void BeginWave()
    {
        if (s_state == GameState.InWave)
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

        Switchboard.WaveStart(m_currentWave);

        m_waveTime = 20;
        s_state = GameState.InWave;
    }

    public void EndWave()
    {
        Switchboard.WaveEnd(m_currentWave+1);
        SwarmerManager.Instance.Reset();

        if (s_state != GameState.LevelOver && m_currentWave == WaveCount)
        {
            Switchboard.Win();
            s_state = GameState.LevelOver;
        }

        if (s_state == GameState.LevelOver)
        {
            return;
        }

        if (LevelData.Money.Length > m_currentWave)
        {
            PlayerMoney.Instance.AddMoney(LevelData.Money[m_currentWave]);
        }

        ++m_currentWave;
        s_state = GameState.Building;
        UpdateWaveUI();
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

public enum GameState
{
    None,
    Building,
    InWave,
    LevelOver
}
