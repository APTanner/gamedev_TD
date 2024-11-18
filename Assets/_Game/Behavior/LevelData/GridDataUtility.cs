using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GridDataUtility
{
    private static Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    public static void InitializePrefabDictionary()
    {
        // Load prefabs from Resources folder or assign them directly
        prefabDictionary["Obstacle"] = Resources.Load<GameObject>("Prefabs/Obstacle");
        prefabDictionary["HQ"] = Resources.Load<GameObject>("Prefabs/HQ");
        // Add other prefabs as needed
    }

    public static GameObject GetPrefabByTypeName(string typeName)
    {
        if (prefabDictionary.TryGetValue(typeName, out GameObject prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogWarning($"No prefab found for type: {typeName}");
            return null;
        }
    }

    public static void SaveGridData(GridManager gridManager, LevelData gridData)
    {
        gridData.gridSize = new Vector2Int(gridManager.Width, gridManager.Height);
        gridData.elements.Clear();

        for (int y = 0; y < gridManager.Height; y++)
        {
            for (int x = 0; x < gridManager.Width; x++)
            {
                Vector2Int coordinates = new Vector2Int(x, y);
                var cell = gridManager.GetCell(coordinates);
                if (!cell.IsEmpty())
                {
                    IGridElement element = cell.Element;
                    GridElementData elementData = new GridElementData
                    {
                        elementType = element.GetType().Name,
                        coordinates = element.Coordinates,
                    };
                    gridData.elements.Add(elementData);
                }
            }
        }

        EditorUtility.SetDirty(gridData);
        AssetDatabase.SaveAssets();
        Debug.Log("Grid data saved.");
    }

    public static void LoadGridData(GridManager gridManager, LevelData gridData)
    {
        if (gridManager == null || gridData == null)
            return;

        // Clear existing elements
        foreach (Transform child in gridManager.transform)
        {
            GameObject.DestroyImmediate(child.gameObject); // Use DestroyImmediate in the editor
        }

        // Load elements from GridData
        foreach (var elementData in gridData.elements)
        {
            GameObject prefab = GetPrefabByTypeName(elementData.elementType);
            if (prefab != null)
            {
                Vector3 position = gridManager.GetCellCenter(elementData.coordinates);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, gridManager.transform);
                instance.transform.position = position;

                if (instance.TryGetComponent<IGridElement>(out var element))
                {
                    element.SetCoordinates(elementData.coordinates);
                    gridManager.GetCell(elementData.coordinates).SetElement(element);
                }

                Undo.RegisterCreatedObjectUndo(instance, "Load Grid Element");
            }
        }

        Debug.Log("Grid data loaded.");
    }


}
