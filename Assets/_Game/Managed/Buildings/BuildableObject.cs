using UnityEngine;

public class BuildableObject : MonoBehaviour, IBuildable
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
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Vector2Int cellCoords = startCoords + new Vector2Int(x, y);
                if (!grid.IsValidCoordinate(cellCoords) || !grid.GetCell(cellCoords).IsEmpty())
                {
                    return false; // Invalid placement
                }
            }
        }
        return true;
    }

    public void Place(Vector2Int startCoords, GridManager grid)
    {
        Coordinates = startCoords;
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Vector2Int cellCoords = startCoords + new Vector2Int(x, y);
                grid.GetCell(cellCoords).SetElement(this);
            }
        }
    }
}
