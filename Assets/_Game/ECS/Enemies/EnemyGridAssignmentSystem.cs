using Latios;
using Latios.Transforms;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public partial struct EnemyGridAssignmentSystem : ISystem, ISystemNewScene
{
    LatiosWorldUnmanaged latiosWorld;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
    }

    public void OnNewScene(ref SystemState state)
    {
        var grid = latiosWorld.worldBlackboardEntity.GetComponentData<EnemyGridDefines>();

        var gridData = new NativeParallelMultiHashMap<int2, Entity>
            (grid.width * grid.height, Allocator.Persistent);

        var offGrid = new NativeList<Entity>(Allocator.Persistent);

        var gridMovement = new NativeArray<EnemyGridData.CellMovementData>
            (grid.width * grid.height, Allocator.Persistent);

        latiosWorld.sceneBlackboardEntity
            .AddOrSetCollectionComponentAndDisposeOld(
                new EnemyGridData{
                    grid           = gridData,
                    gridMovement   = gridMovement,
                });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var grid = latiosWorld.worldBlackboardEntity.GetComponentData<EnemyGridDefines>();
        var gridData = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<EnemyGridData>();

        gridData.grid.Clear();

        // The intent is to pass in a reference, so we shouldn't need to reassign the component to 
        // the scene blackboard

        var populateJob = new PopulateEnemyGridJob
        {
            Grid     = gridData.grid.AsParallelWriter(),
            CellSize = grid.cellSize,
            Width    = grid.width,
            Height   = grid.height
        };

        state.Dependency = populateJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    [WithAll(typeof(Enemy))]
    partial struct PopulateEnemyGridJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int2, Entity>.ParallelWriter Grid;
        public float CellSize;
        public int Width;
        public int Height;

        public void Execute(Entity entity, in WorldTransform transform)
        {
            int x = (int)(transform.position.x / CellSize);
            int y = (int)(transform.position.z / CellSize);

            if (x >= 0 && x < Width && y >= 0 && y < Width)
            {
                Grid.Add(new int2(x, y), entity);
            }
        }
    }
}


