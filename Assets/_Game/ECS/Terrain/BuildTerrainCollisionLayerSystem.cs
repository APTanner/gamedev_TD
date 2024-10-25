using Latios;
using Latios.Kinemation;
using Latios.Psyshock;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct BuildTerrainCollisionLayerSystem : ISystem, ISystemNewScene
{
    LatiosWorldUnmanaged latiosWorld;

    BuildCollisionLayerTypeHandles m_handles;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
    }

    public void OnNewScene(ref SystemState state)
    {
        latiosWorld.sceneBlackboardEntity
            .AddOrSetCollectionComponentAndDisposeOld<TerrainLayer>(default);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        bool shouldUpdate = latiosWorld.worldBlackboardEntity
            .GetComponentData<ColliderUpdateData>().shouldUpdateTerrain;
        if (!shouldUpdate)
        {
            return;
        }


    }
}
