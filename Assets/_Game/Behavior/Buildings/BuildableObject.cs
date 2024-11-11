using UnityEngine;

public class BuildableObject : MonoBehaviour, IBuilding, IGridElement
{
    public virtual GameObject Prefab => gameObject;
    [SerializeField] private Vector2Int size = new Vector2Int(2, 2);
    public virtual Vector2Int Size => size;
    public virtual int Health { get; protected set; } = 1000;
    public bool IsDestroyed => Health <= 0;
    public Vector2Int Coordinates { get; private set; }

    private Vector2Int m_coordinates;

    private GridManager gridManager;
    private BuildingManager buildingManager;

    protected void Awake()
    {
        buildingManager = BuildingManager.Instance;
        gridManager = GridManager.Instance;
        buildingManager.Register(this);
    }

    public void SetCoordinates(Vector2Int coordinates)
    {
        m_coordinates = coordinates;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (IsDestroyed)
        {
            DestroyBuilding();
        }
    }

    public bool CanPlaceAt(Vector2Int startCoords, GridManager grid)
    {
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Vector2Int cellCoords = new Vector2Int(startCoords.x + x, startCoords.y + y);
                if (!grid.IsValidCoordinate(cellCoords) || !grid.GetCell(cellCoords).IsEmpty())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void Place(Vector2Int startCoords, GridManager grid)
    {
        Coordinates = startCoords;
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Vector2Int cellCoords = startCoords + new Vector2Int(x, y);
                grid.GetCell(cellCoords).SetElement(this);
            }
        }
    }

    public void RemoveFromGrid(GridManager grid)
    {
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Vector2Int cellCoords = Coordinates + new Vector2Int(x, y);
                grid.GetCell(cellCoords).ClearElement();
            }
        }
    }

    private void DestroyBuilding()
    {
        RemoveFromGrid(gridManager);
        buildingManager.CleanupBuildings();
    }
}
