using Latios;
using Latios.Psyshock;
using Unity.Jobs;

public partial struct SwarmerCollisionLayer : ICollectionComponent
{
    public CollisionLayer layer;

    public JobHandle TryDispose(JobHandle inputDeps) =>
        layer.IsCreated ? layer.Dispose(inputDeps) : inputDeps;
}