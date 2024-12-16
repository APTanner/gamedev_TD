using UnityEngine;

public class BuildHardcodedBuildings : MonoBehaviour
{
    [Header("Turrets")]
    [Tooltip("Turret prefabs that implement IBuilding.")]
    public GameObject[] turretPrefabs;
    [Tooltip("Coordinates for each turret in the same order as turretPrefabs.")]
    public Vector2Int[] turretCoordinates;

    [Header("Walls")]
    [Tooltip("Wall prefabs that implement IBuilding.")]
    public GameObject wallPrefab;
    [Tooltip("Coordinates for each wall in the same order as wallPrefabs.")]
    public Vector2Int[] wallCoordinates;

    private void Start()
    {
        // Check references
        BuildingManager buildingManager = BuildingManager.Instance;
        GridManager grid = GridManager.Instance;

        if (buildingManager == null || grid == null)
        {
            Debug.LogError("BuildingManager or GridManager not found in the scene.");
            return;
        }

        // Place turrets
        PlaceBuildings(buildingManager, grid, turretPrefabs, turretCoordinates);

        // Place walls
        PlaceWalls(buildingManager, grid, wallPrefab, wallCoordinates);

        Debug.Log("All hardcoded turrets and walls placed successfully.");
    }

    private void PlaceBuildings(BuildingManager buildingManager, GridManager grid, GameObject[] prefabs, Vector2Int[] coords)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.Log("No prefabs to place in this category.");
            return;
        }

        if (coords == null || coords.Length != prefabs.Length)
        {
            Debug.LogWarning("Coordinates array size must match prefabs array size.");
            return;
        }

        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = prefabs[i];
            IBuilding building = prefab.GetComponent<IBuilding>();

            if (building == null)
            {
                Debug.LogError($"Prefab {prefab.name} does not implement IBuilding.");
                continue;
            }

            Vector2Int coord = coords[i];

            buildingManager.SetBuildableObject(building);
            buildingManager.PlaceObject(coord, grid);
            buildingManager.StopPlacingObject();
        }
    }

    private void PlaceWalls(BuildingManager buildingManager, GridManager grid, GameObject prefab, Vector2Int[] coords)
    {
        if (prefab == null)
        {
            Debug.Log("No prefab to place wall.");
            return;
        }

        for (int i = 0; i < coords.Length; i++)
        {
            IBuilding building = prefab.GetComponent<IBuilding>();

            if (building == null)
            {
                Debug.LogError($"Prefab {prefab.name} does not implement IBuilding.");
                continue;
            }

            Vector2Int coord = coords[i];

            buildingManager.SetBuildableObject(building);
            buildingManager.PlaceObject(coord, grid);
            buildingManager.StopPlacingObject();
        }
    }
}
