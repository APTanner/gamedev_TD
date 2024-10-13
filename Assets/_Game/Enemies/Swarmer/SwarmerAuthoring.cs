using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SwarmerAuthoring : MonoBehaviour
{
    class Baker : Baker<SwarmerAuthoring>
    {
        public override void Bake(SwarmerAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new Swarmer());
            AddBuffer<DesiredAvoidance>(e);
        }
    }
}

public struct Swarmer : IComponentData
{
    public float2 target;
}

// Don't store in the chunk
[InternalBufferCapacity(0)]
public struct DesiredAvoidance : IBufferElementData
{
    public float2 value;
}