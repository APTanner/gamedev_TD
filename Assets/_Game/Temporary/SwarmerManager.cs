using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwarmerManager : MonoBehaviour
{
    public static SwarmerManager Instance { get; private set; }

    [Header("Movement")]
    public float MaxSpeed;
    public float Acceleration;
    public float TurnSpeed;
    public float AvoidanceAngle;
    public float ObstacleAvoidDistance;
    public float WhiskerDistance;
    public float NeighborDetectionDistance;
    public float ObstacleAvoidSlowdownRange;
    public float TargetWeight;
    public float AlignmentWeight;
    public float ObstacleWeight;
    public float SeparationWeight;
    public bool DebugMovement = true;

    [Header("Attacking")]
    public float AttackDistance;
    public int AttackDamage;

    [Header("Grid Cells")]
    public float CellSize;
    public int DrawDistance;
    public bool Draw;

    [Header("Prefab")]
    public SwarmerController Prefab;

    private List<SwarmerController> m_swarmers = new();
    private Dictionary<Vector2Int, Vector2> m_averageHeading = new();

    protected void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        SpawnSwarmers(Vector2.zero, 100);
    }

    public void Update()
    {
        if (!Draw)
        {
            return;
        }

        Vector2 bounds = new Vector2(-DrawDistance*CellSize, DrawDistance*CellSize);

        // Horizontal lines
        for (int i = -DrawDistance; i <= DrawDistance; ++i)
        {
            float pos = CellSize * i;
            Debug.DrawLine(new Vector3(bounds.x, 1, pos), new Vector3(bounds.y, 1, pos));
        }

        // Vertical lines
        for (int i = -DrawDistance; i <= DrawDistance; ++i)
        {
            float pos = CellSize*i;
            Debug.DrawLine(new Vector3(pos, 1, bounds.x), new Vector3(pos, 1, bounds.y));
        }
    }

    public void Register(SwarmerController swarmer)
    {
        m_swarmers.Add(swarmer);
    }

    protected void FixedUpdate()
    {
        m_averageHeading.Clear();
        foreach (var swarmer in m_swarmers)
        {
            Vector2Int coord = GetCoord(swarmer.transform.position);
            if (!m_averageHeading.ContainsKey(coord))
            {
                m_averageHeading.Add(coord, new());
            }
            m_averageHeading[coord] += swarmer.transform.forward.xz();
        }

        var keys = m_averageHeading.Keys.ToList();
        foreach (var key in keys)
        {
            m_averageHeading[key] = m_averageHeading[key].normalized;
        }

        List<SwarmerController> swarmersToRemove = new();

        foreach (var swarmer in m_swarmers)
        {
            swarmer.UpdateSwarmer();

            if (swarmer.IsAttacking)
            {
                swarmer.Target.TakeDamage(AttackDamage);
            }

            if (Vector3.SqrMagnitude(SwarmerTarget.Instance.transform.position - swarmer.transform.position) < 1)
            {
                swarmersToRemove.Add(swarmer);
            }
        }
        foreach (var swarmer in swarmersToRemove)
        {
            m_swarmers.Remove(swarmer);
            Destroy(swarmer.gameObject);
        }

        BuildingManager.Instance.CleanupBuildings();
    }

    public Vector2 GetCellHeading(Vector2Int coord)
    {
        return m_averageHeading[coord];
    }

    public Vector2 GetCellHeading(Vector3 pos)
    {
        return m_averageHeading[GetCoord(pos)];
    }

    public Vector2Int GetCoord(Vector3 pos)
    {
        return new(Mathf.FloorToInt(pos.x / CellSize), Mathf.FloorToInt(pos.z / CellSize));
    }

    /// <summary>
    /// Spawns swarmers in a tightly packed square
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="count"></param>
    public void SpawnSwarmers(Vector2 pos, int count)
    {
        const float radius = 0.5f; //hardcoding because this is temporary
        const float spacing = radius * 2;

        Instantiate(Prefab, new Vector3(pos.x, 0.5f, pos.y), Quaternion.identity);

        for (int i = 0, index = 1; index < count; ++i)
        {
            int steps = (i / 2) + 1;
            bool bVertical = i % 2 == 0;
            int sign = steps % 2 == 0 ? -1 : 1;

            Vector2 offset = new Vector2(
                bVertical ? 0 : sign,
                !bVertical ? 0 : sign);

            for (int j = 0; index < count && j < steps; ++j)
            {
                pos += offset * spacing;
                Instantiate(Prefab, new Vector3(pos.x, 0.5f, pos.y), Quaternion.identity);
                ++index;
            }
        }
    }
}


