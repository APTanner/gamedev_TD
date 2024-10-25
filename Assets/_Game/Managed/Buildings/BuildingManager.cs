using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    // will find a better way of doing this...
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public List<GameObject> uiElementsToBlock;

    [SerializeField] private Wall WallPrefab;

    private List<IBuilding> m_buildings = new();

    private bool isBuildingWalls = false;

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
        if (!isBuildingWalls)
        {
            return;
        }
        
        if (!Input.GetMouseButton(0))
        {
            return;
        }

        if (IsPointerOverUIElement())
        {
            Debug.Log("Pointer is over UI");
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

    public void ToggleWallBuilding()
    {
        isBuildingWalls = !isBuildingWalls;
    }

    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        uiRaycaster.Raycast(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (uiElementsToBlock.Contains(result.gameObject))
            {
                return true; 
            }
        }

        return false; 
    }
}
