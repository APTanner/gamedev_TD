using UnityEngine;

public class BuildableObject : MonoBehaviour, IBuildable, IBuilding
{
    public GameObject Prefab => gameObject;

    [SerializeField] private Vector2Int size = new Vector2Int(2, 2); // Editable in Inspector
    public Vector2Int Size => size;

    private bool isDestroyed = false;
    public bool IsDestroyed => isDestroyed;

    public Vector2Int Coordinates { get; private set; }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TakeDamage(int damage)
    {
        // Implement damage handling and destruction
        Debug.Log($"Taking {damage} damage.");
        isDestroyed = true;
    }

    public bool CanPlaceAt(Vector2Int startCoords, GridManager grid)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int cellCoords = new Vector2Int(startCoords.x + x, startCoords.y + y);

                // Debug: Log cell validation
                // Debug.Log($"Checking cell at {cellCoords}: IsEmpty = {grid.GetCell(cellCoords).IsEmpty()}");

                if (!grid.IsValidCoordinate(cellCoords) || !grid.GetCell(cellCoords).IsEmpty())
                {
                    return false; // Placement is invalid if any cell is occupied or out of bounds
                }
            }
        }
        return true;
    }

    public void Place(Vector2Int startCoords, GridManager grid)
    {
        Coordinates = startCoords;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int cellCoords = startCoords + new Vector2Int(x, y);
                //Debug.Log($"Placing object: Occupied cell at {cellCoords}");
                grid.GetCell(cellCoords).SetElement(this);
            }
        }
    }
}
