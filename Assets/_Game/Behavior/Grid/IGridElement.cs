using UnityEngine;

public interface IGridElement 
{
    public Vector2Int Coordinates { get; }

    void SetCoordinates(Vector2Int coordinates);
}

//public struct Obstacle : IGridElement
//{
//    private float m_height;
//    private Vector2Int m_coordinates;

//    public Vector2Int Coordinates => m_coordinates;

//    public Obstacle(float height, Vector2Int coordinates)
//    {
//        m_height = height;
//        m_coordinates = coordinates;
//    }

//    public void SetCoordinates(Vector2Int coordinates)
//    {
//        m_coordinates = coordinates;
//    }

//    public float Height => m_height;
//    public void SetHeight(float height) => m_height = height;
//}

public interface IBuilding: IGridElement
{
    GameObject Prefab { get; }
    Vector2Int Size { get; }
    int Health { get; }
    bool IsDestroyed { get; }
    void TakeDamage(int damage);
    Vector3 GetPosition();
    bool CanPlaceAt(Vector2Int startCoords, GridManager grid);
    void Place(Vector2Int startCoords, GridManager grid);
    void RemoveFromGrid(GridManager grid);
}


