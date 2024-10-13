using Latios;
using Latios.Transforms;
using Unity.Entities;
using UnityEngine;

public partial struct SwarmerSpawnSystem : ISystem
{
    LatiosWorldUnmanaged latiosWorld;

    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
    }

    public void OnUpdate(ref SystemState state)
    {
        Entity prefab = latiosWorld.worldBlackboardEntity.GetComponentData<GlobalSwarmerData>().prefab;

        for (int i = 0; i < 4000; ++i)
        {
            Vector2 randomVector = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));

            Entity e = state.EntityManager.Instantiate(prefab);
            state.EntityManager.GetAspect<TransformAspect>(e).worldPosition = new Unity.Mathematics.float3(
                randomVector.x, 0.5f, randomVector.y);
        }
    }
}
