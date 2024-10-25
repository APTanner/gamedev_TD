using Latios;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct ColliderUpdateData : IComponentData
{
    public bool shouldUpdateTerrain;
    public bool shouldUpdateBuildings;
}

public struct BuildingGridDefines : IComponentData
{
    public float cellSize;
    public int width;
    public int height;
}

public struct EnemyGridDefines : IComponentData
{
    public float cellSize;
    public int width;
    public int height;
}

public partial struct EnemyGridData : ICollectionComponent
{
    public NativeParallelMultiHashMap<int2, Entity> grid;
    public NativeArray<CellMovementData> gridMovement;

    public JobHandle TryDispose(JobHandle inputDeps)
    {
        if (!grid.IsCreated && !gridMovement.IsCreated)
        {
            return inputDeps;
        }

        JobHandle gridHandle = grid.IsCreated ? grid.Dispose(inputDeps) : inputDeps;
        JobHandle offGridHandle = gridMovement.IsCreated ? gridMovement.Dispose(inputDeps) : inputDeps;

        return JobHandle.CombineDependencies(gridHandle, offGridHandle);
    }

    public struct CellMovementData
    {
        public float2 heading;
        public float velocity;
        public int count;
    }
}

