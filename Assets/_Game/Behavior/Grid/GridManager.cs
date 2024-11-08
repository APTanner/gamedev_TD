using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager m_instance;
    public static GridManager Instance => m_instance;

    private GridCell[] m_cells;
    private Vector2Int m_size = new Vector2Int(100, 100);

    public int Width => m_size.x;
    public int Height => m_size.y;

    protected void Awake()
    {
        m_instance = this;
        m_cells = new GridCell[Width * Height];
        for (int y = 0; y < m_size.y; ++y)
        {
            int yOffset = y * m_size.x;
            for (int x = 0; x < m_size.x; ++x)
            {
                m_cells[yOffset + x] = new GridCell(x, y);
            }
        }
    }

    protected void Start()
    {
        GridDebugManager.Instance?.SetDebugPrefabs(this);
    }

    public ref GridCell GetCell(Vector2Int coordinates)
    {
        return ref m_cells[coordinates.y * m_size.x + coordinates.x];
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
            coordinates.y * size +size / 2
        );
    }

    public bool IsValidCoordinate(Vector2Int coordinates)
    {
        return
            coordinates.x >= 0 &&
            coordinates.y >= 0 &&
            coordinates.x < m_size.x &&
            coordinates.y < m_size.y;
    }
}

public class GridData : ScriptableObject
{
    private Vector2Int m_size;
    private GridCell[] m_cells;
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
