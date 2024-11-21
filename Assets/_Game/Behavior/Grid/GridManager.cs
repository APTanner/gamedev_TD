using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager m_instance;
    public static GridManager Instance => m_instance;

    public Vector2Int Size;

    private GridCell[] m_cells;

    public int Width => Size.x;
    public int Height => Size.y;

    protected void Awake()
    {
        m_instance = this;
    }

    public void InitializeLevelGridData(LevelData levelData)
    {
        Size = levelData.Size;
        CreateCells();

        BuildingManager bm = BuildingManager.Instance;
        bm.StartPlacingObject(PrefabManager.Instance.ObstaclePrefab);

        foreach (Vector2Int coord in levelData.Obstacles)
        {
            bm.PlaceObject(coord, this);
        }

        bm.StopPlacingObject();

        bm.StartPlacingObject(PrefabManager.Instance.HQPrefab);
        bm.PlaceObject(levelData.HQCoordinates, this);

        bm.StopPlacingObject();
    }

    private void CreateCells()
    {
        m_cells = new GridCell[Width * Height];

        for (int y = 0; y < Size.y; ++y)
        {
            int yOffset = y * Size.x;
            for (int x = 0; x < Size.x; ++x)
            {
                m_cells[yOffset + x] = new GridCell(x, y);
            }
        }
    }

    public void InitializeEmptyLevel()
    {
        CreateCells();
    }

    protected void Start()
    {
        GridDebugManager.Instance?.SetDebugPrefabs(this);
    }

    public ref GridCell GetCell(Vector2Int coordinates)
    {
        return ref m_cells[coordinates.y * Size.x + coordinates.x];
    }

    public Vector2Int GetCoordinates(Vector3 position)
    {
        // If the grid is not in the center of the world we probably have to change something here
        return new Vector2Int(
            Mathf.FloorToInt(position.x / Defines.BuildingGridCellSize),
            Mathf.FloorToInt(position.z / Defines.BuildingGridCellSize)
        );
    }

    public Vector3 GetCellCenter(Vector2Int coordinates)
    {
        float size = Defines.BuildingGridCellSize;
        return new Vector3(
            coordinates.x * size + size / 2,
            0,
            coordinates.y * size + size / 2
        );
    }

    public bool IsValidCoordinate(Vector2Int coordinates)
    {
        return
            coordinates.x >= 0 &&
            coordinates.y >= 0 &&
            coordinates.x < Size.x &&
            coordinates.y < Size.y;
    }

    public void PopulateGridData(ref LevelData gd)
    {
        List<Vector2Int> obstacleCoords = new();
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                Vector2Int coord = new Vector2Int(x, y);
                if (GetCell(coord).Element is MonoBehaviour mb &&
                    mb.gameObject.layer == Defines.EnvironmentLayer)
                {
                    obstacleCoords.Add(coord);
                }
            }
        }

        gd.Size = Size;
        gd.Obstacles = obstacleCoords.ToArray();
    }
}

public struct GridCell
{
    private IGridElement m_element;
    public readonly Vector2Int Coordinates;
    public GridCell(int x, int y)
    {
        Coordinates = new Vector2Int(x, y);
        m_element = null;
    }

    public IGridElement Element { get { return m_element; } }

    public void SetElement(IGridElement element)
    {
        m_element = element;
    }

    public void ClearElement()
    {
        m_element = null;
    }

    public bool IsEmpty()
    {
        return m_element == null;
    }
}
