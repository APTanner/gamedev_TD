using Latios;
using Latios.Psyshock;
using Latios.Psyshock.Anna;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[BurstCompile]
public partial struct BuildSwarmerCollisionLayerSystem : ISystem, ISystemNewScene
{
    LatiosWorldUnmanaged latiosWorld;

    BuildCollisionLayerTypeHandles m_handles;
    EntityQuery m_query;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
        m_query = state.Fluent().With<Swarmer>(true).PatchQueryForBuildingCollisionLayer().Build();
    }

    public void OnNewScene(ref SystemState state)
    {
        latiosWorld.sceneBlackboardEntity
            .AddOrSetCollectionComponentAndDisposeOld<SwarmerCollisionLayer>(default);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        m_handles.Update(ref state);

        CollisionLayerSettings settings = CoreExtensions.GetPhysicsSettings(latiosWorld)
            .collisionLayerSettings;

        state.Dependency = Physics.BuildCollisionLayer(m_query, in m_handles)
            .WithSettings(settings)
            .ScheduleParallel(out var layer, Allocator.Persistent, state.Dependency);

        latiosWorld.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(
            new SwarmerCollisionLayer { layer = layer });
    }
}
