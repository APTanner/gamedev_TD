using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public List<GameObject> uiElementsToBlock;

    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    private GameObject previewInstance;
    private IBuilding currentBuildable;
    private bool isPlacingObject = false;
    private bool isSellingMode = false;
    private bool isDragging = false;

    private IBuilding hoveredBuilding = null;
    private Material originalMaterial = null;

    private Vector2Int dragStartCoords;
    private List<Vector2Int> dragCoords = new List<Vector2Int>();
    private List<GameObject> previewWallInstances = new List<GameObject>();

    private List<IBuilding> m_buildings = new List<IBuilding>();

    public static BuildingManager Instance { get; private set; }

    protected void Awake()
    {
        Instance = this;
    }

    protected void Update()
    {
        if (isSellingMode)
        {
            HandleSellingHover();
            if (Input.GetMouseButtonDown(0)) // Left-click to sell
            {
                AttemptSellBuilding();
            }
        }
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
        Vector2Int currentCoords = grid.GetCoordinates(mousePosition);

        if (currentBuildable is Wall)
        {
            if (!isDragging)
            {
                ShowStandardPreview(currentCoords, grid);

                if (Input.GetMouseButtonDown(0))
                {
                    dragStartCoords = currentCoords;
                    isDragging = true;
                    ClearStandardPreview();
                }
            }
            else
            {
                HandleWallDragging(currentCoords, grid);
            }
        }
        else
        {
            HandleStandardPlacement(currentCoords, grid);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            ClearWallPreview();
        }
    }

    public void ToggleSellingMode()
    {
        isSellingMode = !isSellingMode;
        ResetHoverEffects();
    }

    private void ShowStandardPreview(Vector2Int currentCoords, GridManager grid)
    {
        Vector3 position = GetAlignedPosition(currentCoords, currentBuildable.Size, grid);
        if (previewInstance == null)
        {
            CreatePreview(currentBuildable.Prefab, validPlacementMaterial);
        }

        bool canPlace = currentBuildable.CanPlaceAt(currentCoords, grid);
        UpdatePreviewPosition(position, canPlace);
    }

    private void ClearStandardPreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
    }

    private void HandleWallDragging(Vector2Int currentCoords, GridManager grid)
    {
        dragCoords = GetLineCoordinates(dragStartCoords, currentCoords);
        UpdateWallPreview(dragCoords, grid);

        if (Input.GetMouseButtonUp(0))
        {
            if (CanPlaceWallLine(dragCoords, grid))
            {
                foreach (var coord in dragCoords)
                {
                    PlaceObject(coord, grid);
                }
            }
            ClearWallPreview();
            isDragging = false;
        }
    }

    private bool CanPlaceWallLine(List<Vector2Int> coords, GridManager grid)
    {
        foreach (var coord in coords)
        {
            if (!grid.IsValidCoordinate(coord) || !grid.GetCell(coord).IsEmpty())
            {
                return false;
            }
        }
        return true;
    }

    private void HandleStandardPlacement(Vector2Int currentCoords, GridManager grid)
    {
        Vector2Int adjustedStartCoords = new Vector2Int(
            currentCoords.x + (currentBuildable.Size.x - 1),
            currentCoords.y + (currentBuildable.Size.y - 1)
        );

        Vector2Int placementStartCoords = new Vector2Int(
            currentCoords.x + (currentBuildable.Size.x - 1) / 2,
            currentCoords.y + (currentBuildable.Size.y - 1) / 2
        );

        Vector3 gridPos = GetAlignedPosition(adjustedStartCoords, currentBuildable.Size, grid);
        bool canPlace = currentBuildable.CanPlaceAt(placementStartCoords, grid);
        UpdatePreviewPosition(gridPos, canPlace);

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PlaceObject(currentCoords, grid);
        }
    }

    private List<Vector2Int> GetLineCoordinates(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> coordinates = new List<Vector2Int>();
        int xDiff = Mathf.Abs(end.x - start.x);
        int yDiff = Mathf.Abs(end.y - start.y);
        int xStep = end.x > start.x ? 1 : -1;
        int yStep = end.y > start.y ? 1 : -1;
        int error = xDiff - yDiff;

        int x = start.x;
        int y = start.y;

        while (true)
        {
            coordinates.Add(new Vector2Int(x, y));
            if (x == end.x && y == end.y) break;

            int error2 = 2 * error;
            if (error2 > -yDiff)
            {
                error -= yDiff;
                x += xStep;
            }
            if (error2 < xDiff)
            {
                error += xDiff;
                y += yStep;
            }
        }

        return coordinates;
    }

    private void UpdateWallPreview(List<Vector2Int> coords, GridManager grid)
    {
        ClearWallPreview();

        // Check if the whole wall line can be placed
        bool canPlaceEntireLine = true;

        foreach (var coord in coords)
        {
            if (!grid.IsValidCoordinate(coord) || !grid.GetCell(coord).IsEmpty())
            {
                canPlaceEntireLine = false;
                break;
            }
        }

        // Use the appropriate material based on whether the entire line can be placed
        Material previewMaterial = canPlaceEntireLine ? validPlacementMaterial : invalidPlacementMaterial;

        foreach (var coord in coords)
        {
            Vector3 position = GetAlignedPosition(coord, currentBuildable.Size, grid);

            if (currentBuildable.Prefab != null)
            {
                GameObject previewWall = Instantiate(currentBuildable.Prefab, position, Quaternion.identity);
                SetPreviewMaterial(previewWall, previewMaterial);
                previewWallInstances.Add(previewWall);
            }
            else
            {
                Debug.LogError("currentBuildable.Prefab is null. Please assign a prefab to the buildable object.");
            }
        }
    }



    private void ClearWallPreview()
    {
        foreach (var instance in previewWallInstances)
        {
            Destroy(instance);
        }
        previewWallInstances.Clear();
    }

    public void PlaceObject(Vector2Int startCoords, GridManager grid)
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
        IBuilding buildableInstance = Instantiate(currentBuildable.Prefab, position, Quaternion.identity).GetComponent<IBuilding>();
        buildableInstance.Place(placementStartCoords, grid);

        Register(buildableInstance);
    }

    public void SetBuildableObject(IBuilding buildablePrefab)
    {
        if (isPlacingObject)
        {
            StopPlacingObject();
        }

        StartPlacingObject(buildablePrefab);
    }

    public void StartPlacingObject(IBuilding buildablePrefab)
    {
        currentBuildable = buildablePrefab;
        isPlacingObject = true;

        CreatePreview(buildablePrefab.Prefab, validPlacementMaterial);
    }

    public void StopPlacingObject()
    {
        isPlacingObject = false;
        DestroyPreview();
        ClearWallPreview();
    }

    private void DestroyPreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
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
        List<IBuilding> destroyedBuildings = new List<IBuilding>();
        foreach (IBuilding building in m_buildings)
        {
            if (building.IsDestroyed)
            {
                destroyedBuildings.Add(building);
            }
        }

        foreach (IBuilding building in destroyedBuildings)
        {
            m_buildings.Remove(building);
            building.RemoveFromGrid(GridManager.Instance);
            Destroy((building as Component).gameObject);
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

        TurretController turretPreview = previewInstance.GetComponent<TurretController>();
        if (turretPreview != null)
        {
            turretPreview.IsPreview = true;
        }

        SetPreviewMaterial(previewInstance, initialMaterial);
    }

    private void SetPreviewMaterial(GameObject instance, Material material)
    {
        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>())
        {
            renderer.material = material;
        }
    }

    private void UpdatePreviewPosition(Vector3 position, bool canPlace)
    {
        if (previewInstance != null)
        {
            previewInstance.transform.position = position;
            SetPreviewMaterial(previewInstance, canPlace ? validPlacementMaterial : invalidPlacementMaterial);
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

    private void HandleSellingHover()
    {
        Vector3 mousePosition = WorldMousePosition.Instance.Position;

        if (mousePosition == Vector3.zero || IsPointerOverUIElement())
        {
            ResetHoverEffects();
            return;
        }

        Vector2Int currentCoords = GridManager.Instance.GetCoordinates(mousePosition);
        GridCell cell = GridManager.Instance.GetCell(currentCoords);
        IBuilding building = cell.Element as IBuilding;

        if (building != hoveredBuilding)
        {
            ResetHoverEffects();
            if (building != null && building.IsSellable)
            {
                hoveredBuilding = building;
                ApplyHoverMaterial(hoveredBuilding, invalidPlacementMaterial);
            }
        }
    }

    private void AttemptSellBuilding()
    {
        if (hoveredBuilding != null && hoveredBuilding.IsSellable)
        {
            int refundAmount = hoveredBuilding.Price / 2; // Example refund logic
            PlayerMoney.Instance.AddMoney(refundAmount); // Adjust based on your resource system

            hoveredBuilding.RemoveFromGrid(GridManager.Instance);
            Destroy((hoveredBuilding as MonoBehaviour).gameObject);

            Debug.Log($"Sold building for {refundAmount} gold.");
        }
        else
        {
            Debug.Log("No sellable building hovered.");
        }

        ResetHoverEffects();
    }

    private void ApplyHoverMaterial(IBuilding building, Material hoverMaterial)
    {
        if (building is MonoBehaviour component)
        {
            foreach (Renderer renderer in component.GetComponentsInChildren<Renderer>())
            {
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.material; // Store the original material
                }
                renderer.material = hoverMaterial; // Apply hover material
            }
        }
    }

    private void ResetHoverEffects()
    {
        if (hoveredBuilding != null && hoveredBuilding is MonoBehaviour component)
        {
            foreach (Renderer renderer in component.GetComponentsInChildren<Renderer>())
            {
                if (originalMaterials.TryGetValue(renderer, out Material originalMaterial))
                {
                    renderer.material = originalMaterial; // Restore original material
                }
            }
        }

        hoveredBuilding = null;
        originalMaterials.Clear(); // Clear the dictionary to avoid stale data
    }
}
