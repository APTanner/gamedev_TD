using UnityEditor;
using UnityEngine;

public class GridEditorWindow : EditorWindow
{
    private GridManager gridManager;
    private GameObject selectedPrefab;
    private Vector2Int selectedCoordinates;

    [MenuItem("Tools/Grid Editor")]
    public static void ShowWindow()
    {
        GetWindow<GridEditorWindow>("Grid Editor");
    }

    private void OnEnable()
    {
        // Initialize the prefab dictionary
        GridDataUtility.InitializePrefabDictionary();
    }

    private void OnGUI()
    {
        gridManager = (GridManager)EditorGUILayout.ObjectField("Grid Manager", gridManager, typeof(GridManager), true);
        selectedPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab to Place", selectedPrefab, typeof(GameObject), false);

        if (gridManager == null || selectedPrefab == null)
        {
            EditorGUILayout.HelpBox("Assign both Grid Manager and Prefab to place.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Place Prefab at Selected Coordinates"))
        {
            PlacePrefab();
        }
    }

    private void OnSceneGUI()
    {
        if (gridManager == null)
            return;

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 hitPoint = hit.point;
                selectedCoordinates = gridManager.GetCoordinates(hitPoint);
                Repaint();
            }
        }
    }

    private void PlacePrefab()
    {
        if (gridManager == null || selectedPrefab == null)
            return;

        Vector3 position = gridManager.GetCellCenter(selectedCoordinates);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab, gridManager.transform);
        instance.transform.position = position;

        if (instance.TryGetComponent<IGridElement>(out var element))
        {
            element.SetCoordinates(selectedCoordinates);
            gridManager.GetCell(selectedCoordinates).SetElement(element);
        }

        Undo.RegisterCreatedObjectUndo(instance, "Place Grid Element");
        EditorUtility.SetDirty(gridManager);
    }
}
