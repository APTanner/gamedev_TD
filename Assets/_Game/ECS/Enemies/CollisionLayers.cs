using Latios;
using Latios.Psyshock;
using Unity.Jobs;

public partial struct EnemiesHitBoxLayer : ICollectionComponent
{
    public CollisionLayer layer;

    public JobHandle TryDispose(JobHandle inputDeps) =>
        layer.IsCreated ? layer.Dispose(inputDeps) : inputDeps;
}

public partial struct WalkerInteractionLayer : ICollectionComponent
{
    public CollisionLayer layer;
    public JobHandle TryDispose(JobHandle inputDeps) =>
        layer.IsCreated ? layer.Dispose(inputDeps) : inputDeps;
}

public partial struct CrawlerInteractionLayer : ICollectionComponent
{
    public CollisionLayer layer;
    public JobHandle TryDispose(JobHandle inputDeps) =>
        layer.IsCreated ? layer.Dispose(inputDeps) : inputDeps;
}