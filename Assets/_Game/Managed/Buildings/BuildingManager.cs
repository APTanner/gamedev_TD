using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public List<GameObject> uiElementsToBlock;

    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    private GameObject previewInstance;
    private IBuildable currentBuildable;
    private bool isPlacingObject = false;

    private List<IBuilding> m_buildings = new(); 

    public static BuildingManager Instance { get; private set; }

    protected void Awake()
    {
        Instance = this;
    }


    protected void Update()
    {
        if (!isPlacingObject || currentBuildable == null)
        {
            return;
        }

        if (IsPointerOverUIElement())
        {
            return;
        }

        Vector3 mousePosition = WorldMousePosition.Instance.Position;
        if (mousePosition == Vector3.zero)
        {
            return;
        }

        GridManager grid = GridManager.Instance;
        Vector2Int startCoords = grid.GetCoordinates(mousePosition);

        // Adjust startCoords to represent the top-left corner based on object size
        Vector2Int adjustedStartCoords = new Vector2Int(
            startCoords.x + (currentBuildable.Size.x - 1),
            startCoords.y + (currentBuildable.Size.y - 1)
        );

        Vector2Int placementStartCoords = new Vector2Int(
            startCoords.x + (currentBuildable.Size.x - 1) /2,
            startCoords.y + (currentBuildable.Size.y - 1) /2
        );

        Vector3 gridPos = GetAlignedPosition(adjustedStartCoords, currentBuildable.Size, grid);

        // Check if the entire area for the buildable is valid
        bool canPlace = currentBuildable.CanPlaceAt(placementStartCoords, grid);
        UpdatePreviewPosition(gridPos, canPlace);

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PlaceObject(startCoords, grid);

        }
    }

    public void SetBuildableObject(IBuildable buildablePrefab)
    {
        if (isPlacingObject)
        {
            StopPlacingObject(); 
        }

        StartPlacingObject(buildablePrefab); 
    }

    public void StartPlacingObject(IBuildable buildablePrefab)
    {
        currentBuildable = buildablePrefab;
        isPlacingObject = true;

        CreatePreview(buildablePrefab.Prefab, validPlacementMaterial);
    }

    public void StopPlacingObject()
    {
        isPlacingObject = false;
        DestroyPreview();
    }

    private void PlaceObject(Vector2Int startCoords, GridManager grid)
    {
        Vector2Int adjustedStartCoords = new Vector2Int(
            startCoords.x + (currentBuildable.Size.x - 1),
            startCoords.y + (currentBuildable.Size.y - 1)
        );

        Vector2Int placementStartCoords = new Vector2Int(
            startCoords.x + (currentBuildable.Size.x - 1) / 2,
            startCoords.y + (currentBuildable.Size.y - 1) / 2
        );

        Vector3 position = GetAlignedPosition(adjustedStartCoords, currentBuildable.Size, grid);
        IBuildable buildableInstance = Instantiate(currentBuildable.Prefab, position, Quaternion.identity).GetComponent<IBuildable>();
        buildableInstance.Place(placementStartCoords, grid);

        // Register the placed building as an IBuilding for cleanup tracking
        Register((IBuilding)buildableInstance);
    }

    public void Register(IBuilding building)
    {
        if (!m_buildings.Contains(building))
        {
            m_buildings.Add(building);
        }
    }

    public void CleanupBuildings()
    {
        List<IBuilding> destroyedBuildings = new();
        foreach (IBuilding building in m_buildings)
        {
            if (building.IsDestroyed)
            {
                Debug.Log("Destroying building");
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

    private Vector3 GetAlignedPosition(Vector2Int startCoords, Vector2Int objectSize, GridManager grid)
    {

        Vector3 cellCenter = grid.GetCellCenter(startCoords);
        float offsetX = (objectSize.x - 1); 
        float offsetY = (objectSize.y - 1); 


        return new Vector3(cellCenter.x - offsetX, cellCenter.y, cellCenter.z - offsetY);
    }

    private void CreatePreview(GameObject prefab, Material initialMaterial)
    {
        previewInstance = Instantiate(prefab);
        SetPreviewMaterial(initialMaterial);
    }

    private void UpdatePreviewPosition(Vector3 position, bool canPlace)
    {
        Vector3 alignedPosition = GetAlignedPosition(
            GridManager.Instance.GetCoordinates(position),
            currentBuildable.Size,
            GridManager.Instance
        );

        previewInstance.transform.position = alignedPosition;
        SetPreviewMaterial(canPlace ? validPlacementMaterial : invalidPlacementMaterial);
    }

    private void SetPreviewMaterial(Material material)
    {
        foreach (Renderer renderer in previewInstance.GetComponentsInChildren<Renderer>())
        {
            renderer.material = material;
        }
    }

    private void DestroyPreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
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
