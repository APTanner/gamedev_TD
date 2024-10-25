using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class GridDebugManager : MonoBehaviour
{
    [SerializeField] private GridDebug Prefab;

    private static GridDebugManager m_instance;
    public static GridDebugManager Instance => m_instance;

    private void Awake()
    {
        m_instance = this;
    }

    private GridDebug[] m_grid;
    private Vector2Int m_size;

    public void SetDebugPrefabs(GridManager grid)
    {
        m_grid = new GridDebug[grid.Height * grid.Width];
        m_size = new Vector2Int(grid.Width, grid.Height);

        for (int y = 0; y < grid.Height; ++y)
        {
            for (int x = 0; x < grid.Width; ++x)
            {
                GridDebug gd = Instantiate(Prefab, transform);
                m_grid[m_size.x * y + x] = gd;
                gd.SetCoordinates(new Vector2Int(x, y));
                float size = Defines.BuildingGridCellSize;
                Vector3 worldPosition = new Vector3(
                    x * size + size/2,
                    0.01f,
                    y * size + size/2
                );
                gd.transform.position = worldPosition;
                gd.transform.localScale = Vector3.one * size;
            }
        }
    }

    public GridDebug GetGridDebug(Vector2Int coordinates)
    {
        return m_grid[coordinates.y * m_size.x + coordinates.x];
    }
}
