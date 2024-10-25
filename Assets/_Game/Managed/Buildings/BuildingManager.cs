using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private Wall WallPrefab;

    private List<IBuilding> m_buildings = new();

    public static BuildingManager Instance { get; private set; }

    protected void Awake()
    {
        Instance = this;
    }

    public void Register(IBuilding building)
    {
        m_buildings.Add(building);
    }

    protected void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            return;
        }

        Vector3 mousePosition = WorldMousePosition.Instance.Position;
        if (mousePosition == Vector3.zero)
        {
            return;
        }

        GridManager grid = GridManager.Instance;
        Vector2Int coords = grid.GetCoordinates(mousePosition);
        if (!grid.IsValidCoordinate(coords))
        {
            return;
        }

        ref GridCell cell = ref grid.GetCell(coords);
        if (!cell.IsEmpty())
        {
            Debug.Log($"The cell [{coords.ToString()}] already had something on it");
            return;
        }

        Vector3 pos = grid.GetCellCenter(coords);

        Wall wall = Instantiate(WallPrefab, pos, Quaternion.identity);
        wall.SetCoordinates(coords);
        cell.SetElement(wall);
    }

    public void CleanupBuildings()
    {
        List<IBuilding> destroyedBuildings = new();
        foreach (IBuilding building in m_buildings)
        {
            if (building.IsDestroyed)
            {
                Debug.Log("Destroying wall");
                destroyedBuildings.Add(building);
            }
        }

        GridManager grid = GridManager.Instance;
        foreach (IBuilding building in destroyedBuildings)
        {
            m_buildings.Remove(building);
            grid.GetCell(building.Coordinates).ClearElement();
            Component c = building as Component;
            Destroy(c.gameObject);
        }
    }
}
