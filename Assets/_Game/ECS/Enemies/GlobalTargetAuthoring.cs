using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GlobalTargetAuthoring : MonoBehaviour
{
    public Vector2 Target;

    public class Baker : Baker<GlobalTargetAuthoring>
    {
        public override void Bake(GlobalTargetAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new GlobalTarget { target = authoring.Target });
        }
    }
}

public struct GlobalTarget : IComponentData
{
    public float2 target;
}
