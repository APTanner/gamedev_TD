using Latios;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Latios.Psyshock.Anna;
using Latios.Transforms;
using static UnityEngine.EventSystems.EventTrigger;

[BurstCompile]
public partial struct EnemyGridHeadingSystem : ISystem
{
    LatiosWorldUnmanaged latiosWorld;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var grid = latiosWorld.worldBlackboardEntity.GetComponentData<EnemyGridDefines>();
        var gridData = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<EnemyGridData>();

        var headingsJob = new SumCellHeadingsJob
        {
            GridMovement = gridData.gridMovement,
            CellSize = grid.cellSize,
            Width    = grid.width,
            Height   = grid.height
        };

        state.Dependency = headingsJob.Schedule(state.Dependency);
        state.Dependency.Complete();

        var velocitiesJob = new CalculateCellVelocitiesJob { GridMovement = gridData.gridMovement };
        state.Dependency = velocitiesJob.Schedule(gridData.gridMovement.Length, 100);
    }

    [BurstCompile]
    [WithAll(typeof(Enemy))]
    partial struct SumCellHeadingsJob : IJobEntity
    {
        public NativeArray<EnemyGridData.CellMovementData> GridMovement;
        public float CellSize;
        public int Width;
        public int Height;

        public void Execute(in RigidBody rb, in WorldTransform transform)
        {
            int x = (int)(transform.position.x / CellSize);
            int y = (int)(transform.position.z / CellSize);

            if (x >= 0 && x < Width && y >= 0 && y < Width)
            {
                var data = GridMovement[y * Width + x];
                ++data.count;
                data.heading += rb.velocity.linear.xz;
            }
        }
    }

    [BurstCompile]
    partial struct CalculateCellVelocitiesJob : IJobParallelFor
    {
        public NativeArray<EnemyGridData.CellMovementData> GridMovement;

        public void Execute(int index)
        {
            var data = GridMovement[index];
            float velocity = data.count > 0 ? math.length(data.heading) : 0;
            data.heading = data.count >= 0 ? data.heading / velocity : float2.zero;
            data.velocity = velocity;
            GridMovement[index] = data;
        }
    }
}
