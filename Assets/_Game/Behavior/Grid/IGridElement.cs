using UnityEngine;

public interface IGridElement 
{
    public Vector2Int Coordinates { get; }
}

public struct Obstacle : IGridElement
{
    private float m_height;
    private Vector2Int m_coordinates;

    public Vector2Int Coordinates => m_coordinates;

    public Obstacle(float height, Vector2Int coordinates)
    {
        m_height = height;
        m_coordinates = coordinates;
    }
}

public interface IBuilding: IGridElement
{
    GameObject Prefab { get; }
    Vector2Int Size { get; }
    int Health { get; }
    bool IsDestroyed { get; }
    Vector2Int Coordinates { get; }

    void SetCoordinates(Vector2Int coordinates);
    void TakeDamage(int damage);
    Vector3 GetPosition();
    bool CanPlaceAt(Vector2Int startCoords, GridManager grid);
    void Place(Vector2Int startCoords, GridManager grid);
    void RemoveFromGrid(GridManager grid);
}


