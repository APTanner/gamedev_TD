using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Grid/Level Data")]
public class LevelData : ScriptableObject
{
    public Vector2Int gridSize = new Vector2Int(100, 100);
    public List<GridElementData> elements = new List<GridElementData>();

    // New fields for enemies and waves
    public List<EnemySpawnData> enemySpawns = new List<EnemySpawnData>();
    public List<WaveData> waves = new List<WaveData>();
}

[System.Serializable]
public class GridElementData
{
    public string elementType;
    public Vector2Int coordinates;
}

[System.Serializable]
public class EnemySpawnData
{
    public Vector2Int spawnCoordinates;
    public string enemyType;
    public int enemyCount;
}

[System.Serializable]
public class WaveData
{
    public int waveNumber;
    public List<EnemySpawnData> enemiesInWave;
}

