using Latios.Psyshock;
using Latios.Psyshock.Anna;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PhysicsSettingsAuthoring : MonoBehaviour
{
    [Header("World Colliders")]
    [Tooltip("X-Width, Height, Z-Width")]
    public Vector3 WorldBounds = new float3(100, 100, 100);
    public Vector3Int Subdivisions = new Vector3Int(2, 1, 5);

    [Header("Physics")]
    public Vector3 Gravity = new Vector3(0, -1, 0);
    public float linearDamping = 0.05f;
    public float angularDamping = 0.05f;
    public int numIterations = 4;

    public class Baker : Baker<PhysicsSettingsAuthoring>
    {
        public override void Bake(PhysicsSettingsAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);


            float xCoord = authoring.WorldBounds.x / 2f;
            float zCoord = authoring.WorldBounds.z / 2f;
            float yCoord = authoring.WorldBounds.y;
            AddComponent(e, new PhysicsSettings
            {
                collisionLayerSettings = new CollisionLayerSettings
                {
                    worldAabb = new Aabb(new float3(-xCoord, 0, -zCoord),
                                         new float3(xCoord, yCoord, zCoord)),
                    worldSubdivisionsPerAxis = new Unity.Mathematics.int3(2, 1, 5)
                },
                gravity = authoring.Gravity,
                linearDamping          = (half)0.05f,
                angularDamping         = (half)0.05f,
                numIterations          = 2
            });
        }
    }
}
