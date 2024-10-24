public struct CrawlerData
{
    public float speed;
    public float acceleration;
    public float turnSpeed;
    public float targetWeight;
    public float alignmentWeight;
    public float obstacleWeight;
    public float separationWeight;
    public float pathAvoidAngle;
    public float pathViewDistance;
    public float obstacleSlowdownDistance;
    public float obstacleAvoidancePathWhiskerCountPerSide;
    public float obstacleAvoidancePathTestingDistance;
    public float wallBypassTestingDistance;
    public float interactionDistance;

    public bool bAvoidCrawlers;
    public bool bAvoidWalkers;
}

public struct EnemyHealthData
{
    public int health;
    public int armor;
}
