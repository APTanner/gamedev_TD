using Unity.Entities;
using UnityEngine;

public class SwarmerGlobalDataAuthoring : MonoBehaviour
{
    [Header("Swarmer Defines")]
    public float ViewRange;
    public float AvoidanceRange;

    [Header("Movement Defines")]
    public float Acceleration;
    public float MaxSpeed;
    public float RotationRate;
    public float AvoidanceStrength;

    [Header("Swarmer Prefab")]
    public SwarmerAuthoring Prefab;

    class Baker : Baker<SwarmerGlobalDataAuthoring>
    {
        public override void Bake(SwarmerGlobalDataAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);
            Entity prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);
            float colliderRadius = GetComponent<SphereCollider>(authoring.Prefab).radius;

            AddComponent(e, new GlobalSwarmerData
            {
                viewRange = authoring.ViewRange,
                avoidanceRange = authoring.AvoidanceRange,
                radius = colliderRadius,
                acceleration = authoring.Acceleration,
                maxSpeed = authoring.MaxSpeed,
                rotationRate = authoring.RotationRate,
                avoidanceStrength = authoring.AvoidanceStrength,
                prefab = prefab,
            });
        }
    }
}

public struct GlobalSwarmerData : IComponentData
{
    public float viewRange;
    public float avoidanceRange;

    public float radius;

    public float acceleration;
    public float maxSpeed;
    public float rotationRate;

    public float avoidanceStrength;

    public Entity prefab;
}
