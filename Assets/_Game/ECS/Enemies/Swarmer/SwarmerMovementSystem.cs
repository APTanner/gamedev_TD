using Latios;
using Latios.Psyshock;
using Latios.Psyshock.Anna;

using Latios.Transforms;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Collider = Latios.Psyshock.Collider;
using Physics = Latios.Psyshock.Physics;
using SphereCollider = Latios.Psyshock.SphereCollider;

public partial struct SwarmerMovementSystem : ISystem
{
    LatiosWorldUnmanaged latiosWorld;
    EntityQuery m_query;

    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
        m_query = SystemAPI.QueryBuilder().WithAll<Swarmer>().Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        GlobalSwarmerData swarmerData = latiosWorld.worldBlackboardEntity
            .GetComponentData<GlobalSwarmerData>();

        // Build avoidance collision layer
        Collider collider = new SphereCollider(
            float3.zero, swarmerData.radius + swarmerData.avoidanceRange / 2f);

        var bodies = CollectionHelper.CreateNativeArray<ColliderBody>
            (m_query.CalculateEntityCount(),
            state.WorldUpdateAllocator,
            NativeArrayOptions.UninitializedMemory);

        var createBodiesJob = new CreateBodies
        {
            Bodies = bodies,
            Collider = collider
        };

        state.Dependency = createBodiesJob.ScheduleParallel(state.Dependency);

        CollisionLayerSettings settings = CoreExtensions.GetPhysicsSettings(latiosWorld)
            .collisionLayerSettings;

        state.Dependency = Physics.BuildCollisionLayer(bodies)
            .WithSettings(settings)
            .ScheduleParallel(out var layer, state.WorldUpdateAllocator, state.Dependency);

        var processor = new AvoidanceProcessor
        {
            AvoidanceLookup = SystemAPI.GetBufferLookup<DesiredAvoidance>(),
            AvoidanceRange = swarmerData.avoidanceRange,
            Radius = swarmerData.radius,
        };

        state.Dependency = Physics.FindPairs(layer, processor).ScheduleParallel(state.Dependency);

        // Calculate movement
        float2 target = latiosWorld.sceneBlackboardEntity.GetComponentData<GlobalTarget>().target;
        var calculateMovementJob = new CalculateMovement
        {
            Target = target,
            SwarmerData = swarmerData,
            DeltaTime = SystemAPI.Time.DeltaTime,
        };

        state.Dependency = calculateMovementJob.ScheduleParallel(state.Dependency);
    }
}

partial struct CreateBodies : IJobEntity
{
    public NativeArray<ColliderBody> Bodies;
    public SphereCollider Collider;
    public void Execute(Entity entity,
                        [EntityIndexInQuery] int index,
                        in WorldTransform transform,
                        ref DynamicBuffer<DesiredAvoidance> avoid)
    {
        Bodies[index] = new ColliderBody
        {
            collider = Collider,
            entity = entity,
            transform = new TransformQvvs(transform.position, quaternion.identity),
        };

        avoid.Clear();
    }
}

struct AvoidanceProcessor : IFindPairsProcessor
{
    public PhysicsBufferLookup<DesiredAvoidance> AvoidanceLookup;
    public float AvoidanceRange;
    public float Radius;

    public void Execute(in FindPairsResult result)
    {
        float3 pA = result.transformA.position;
        float3 pB = result.transformB.position;

        float2 posA = new float2(pA.x, pA.z);
        float2 posB = new float2(pB.x, pB.z);

        float dist = math.distance(posA, posB) - math.EPSILON;
        float2 AtoB = (posB - posA) / dist;
        float test = dist - Radius * 2;

        // This will end up being >1 if the spheres are intersecting 
        float mag = (AvoidanceRange - test) / AvoidanceRange;
        // Depending on collisions the distance can be farther than expected.
        // Make sure this doesn't cause negative magnitudes
        mag = math.max(mag, 0);
        // Steeper slope
        mag *= mag * mag * mag * mag;

        if (mag > 0)
        {
            AvoidanceLookup[result.entityA].Add(new DesiredAvoidance
            {
                value = -AtoB * mag
            });

            AvoidanceLookup[result.entityB].Add(new DesiredAvoidance
            {
                value = AtoB * mag
            });
        }
    }
}

partial struct CalculateMovement : IJobEntity
{
    public float2 Target;
    public GlobalSwarmerData SwarmerData;
    public float DeltaTime;

    public void Execute(in DynamicBuffer<DesiredAvoidance> avoidance,
                        ref Swarmer swarmer,
                        in WorldTransform transform,
                        ref RigidBody rb)
    {
        // If we're not on the ground we can't move
        if (transform.position.y > SwarmerData.radius + .01f)
        {
            return;
        }

        float2 pos = MathFunctions.FlattenFloat3(transform.position);
        float2 velocity = MathFunctions.FlattenFloat3(rb.velocity.linear);

        float2 dv = float2.zero;

        float2 desiredAvoidance = float2.zero;
        for (int i = 0; i < avoidance.Length; ++i)
        {
            desiredAvoidance += avoidance[i].value;
        }

        float avoidanceStrength = math.length(desiredAvoidance) + math.EPSILON;

        // SCALES WITH AMOUNT TO AVOID - LEADS TO BUNCHING UP
        //float2 avoidanceDv = MathFunctions.AccelerateTowards(
        //    desiredAvoidance / avoidanceStrength,
        //    velocity,
        //    1f,
        //    SwarmerData.maxSpeed)
        //    * avoidanceStrength * SwarmerData.avoidanceStrength;

        // JUST PICKS A DIRECTION - LEADS TO ORDERED PATTERNS
        float2 avoidanceDv = MathFunctions.AccelerateTowards(
            desiredAvoidance / avoidanceStrength,
            velocity,
            SwarmerData.acceleration,
            SwarmerData.maxSpeed)
            * avoidanceStrength * SwarmerData.avoidanceStrength;

        float2 targetDv = MathFunctions.AccelerateTowards(
            math.normalizesafe(Target - pos),
            velocity,
            SwarmerData.acceleration,
            SwarmerData.maxSpeed);

        dv += avoidanceDv;
        dv += targetDv;

        //UnityEngine.Debug.Log($"targetDv direction {math.normalizesafe(targetDv)} targetDirection: {math.normalizesafe(Target - pos)}");

        velocity += dv * DeltaTime;
        velocity = MathFunctions.ClampMagnitude(velocity, SwarmerData.maxSpeed);
        rb.velocity.linear = new float3(velocity.x, rb.velocity.linear.y, velocity.y);
    }
}

