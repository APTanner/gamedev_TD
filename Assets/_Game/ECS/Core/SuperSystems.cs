using Latios;
using Latios.Psyshock.Anna.Systems;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(AnnaSuperSystem))]
public partial class FixedUpdateGameBehavior : RootSuperSystem
{
    protected override void CreateSystems()
    {
        //GetOrCreateAndAddUnmanagedSystem<BuildSwarmerCollisionLayerSystem>();
        //GetOrCreateAndAddManagedSystem<InitializationSuperSystem>();
        //GetOrCreateAndAddUnmanagedSystem<SwarmerMovementSystem>();
    }
}

public partial class InitializationSuperSystem : SuperSystem
{
    bool m_bHasRun = false;
    protected override void CreateSystems()
    {
        //GetOrCreateAndAddUnmanagedSystem<SwarmerSpawnSystem>();
    }

    public override bool ShouldUpdateSystem()
    {
        if (!m_bHasRun)
        {
            m_bHasRun = true;
            return true;
        }
        return false;
    }
}
