using System;
using UnityEngine;

public class LevelData : ScriptableObject
{
    public Vector2Int Size;
    public Vector2Int[] Obstacles;
    public Vector2Int HQCoordinates;
    public SpawnerData[] Spawners;
    public int[] Money;
}

[Serializable]
public struct SpawnerData
{
    public int EnemyCount;
    public int WaveNum;
    public Vector3 Position;
    public Quaternion Rotation;
}