using Unity.Collections;
using Unity.Entities;
using UnityEngine;


/// <summary>
/// Should be attached to a world blackboard entity
/// </summary>
public class SwarmerDataAuthoring : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float Speed;
    [SerializeField] private float Acceleration;
    [SerializeField] private float TurnSpeed;
    [SerializeField] private bool AvoidWalkers;
    [SerializeField] private bool AvoidCrawlers;

    [Header("Movement Decision Weights")]
    [SerializeField] private float TargetWeight;
    [SerializeField] private float AlignmentWeight;
    [SerializeField] private float ObstacleWeight;
    [SerializeField] private float SeparationWeight;

    [Header("Senses")]
    [SerializeField] private float PathAvoidAngle;
    [SerializeField] private float PathViewDistance;
    [SerializeField] private float ObstacleSlowdownDistance;
    [SerializeField] private float ObstacleAvoidancePathWhiskerCountPerSide;
    [SerializeField] private float ObstacleAvoidancePathTestingDistance;
    [SerializeField] private float WallBypassTestingDistance;

    [Header("Enemy Avoidance/Interaction Distance")]
    [SerializeField] private float SwarmerInteractionDistance;

    [Header("Swarmer Prefab")]
    [SerializeField] private SwarmerAuthoring Prefab;

    public class Baker : Baker<SwarmerDataAuthoring>
    {
        public override void Bake(SwarmerDataAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);

            SwarmerData swarmerData = new();
            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            ref CrawlerData data = ref blobBuilder.ConstructRoot<CrawlerData>();
            data = new CrawlerData
            {
                speed                     = authoring.Speed,
                acceleration              = authoring.Acceleration,
                turnSpeed                 = authoring.TurnSpeed,
                targetWeight              = authoring.TargetWeight,
                alignmentWeight           = authoring.AlignmentWeight,
                obstacleWeight            = authoring.ObstacleWeight,
                separationWeight          = authoring.SeparationWeight,
                pathAvoidAngle            = authoring.PathAvoidAngle,
                pathViewDistance          = authoring.PathViewDistance,
                obstacleSlowdownDistance  = authoring.ObstacleSlowdownDistance,
                interactionDistance       = authoring.SwarmerInteractionDistance,
                wallBypassTestingDistance = authoring.WallBypassTestingDistance,

                obstacleAvoidancePathWhiskerCountPerSide = authoring.ObstacleAvoidancePathWhiskerCountPerSide,
                obstacleAvoidancePathTestingDistance     = authoring.ObstacleAvoidancePathTestingDistance,

                bAvoidCrawlers = authoring.AvoidCrawlers,
                bAvoidWalkers  = authoring.AvoidWalkers,
            };

            swarmerData.dataReference = blobBuilder.CreateBlobAssetReference<CrawlerData>(Allocator.Persistent);
            blobBuilder.Dispose();
            AddBlobAsset(ref swarmerData.dataReference, out Unity.Entities.Hash128 _);
            swarmerData.prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);

            AddComponent(e, swarmerData);
        }
    }
}

public struct SwarmerData : IComponentData
{
    public BlobAssetReference<CrawlerData> dataReference;
    public Entity prefab;
}
