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

public interface IBuilding : IGridElement
{
    public Vector3 GetPosition();
    public void TakeDamage(int damage);
    public bool IsDestroyed { get; }
}