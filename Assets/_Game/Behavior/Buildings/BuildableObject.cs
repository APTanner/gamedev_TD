using UnityEngine;

public class BuildableObject : MonoBehaviour, IBuilding, IGridElement
{
    public virtual GameObject Prefab => gameObject;
    [SerializeField] private Vector2Int size = new Vector2Int(2, 2);
    public virtual Vector2Int Size => size;
    [SerializeField] private int health = 1000;
    public virtual int Health { get; protected set; } = 1000;
    [SerializeField] private int price = 1000;
    public virtual int Price
    {
        get => price; // Return the serialized field value
        protected set => price = value; // Allow runtime overrides
    }
    public bool IsDestroyed => Health <= 0;
    [SerializeField] private bool isSellable = true;
    public virtual bool IsSellable { get; protected set; } = true;
    public Vector2Int Coordinates { get; private set; }

    private Vector2Int m_coordinates;

    private GridManager gridManager;
    private BuildingManager buildingManager;

    protected virtual void Awake()
    {
        buildingManager = BuildingManager.Instance;
        gridManager = GridManager.Instance;
        buildingManager.Register(this);
    }

    protected void OnDestroy()
    {
        if (Application.isPlaying)
        {
            buildingManager.Deregister(this);
        }
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
}
