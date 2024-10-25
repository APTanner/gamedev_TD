using UnityEngine;

public class Wall : MonoBehaviour, IBuilding
{
    private int m_health = 10000;

    private BuildingManager m_manager;

    private Vector2Int m_coordinates;

    bool IBuilding.IsDestroyed => m_health <= 0;

    public Vector2Int Coordinates => m_coordinates;

    public void SetCoordinates(Vector2Int coordinates)
    {
        m_coordinates = coordinates;
    }

    protected void Awake()
    {
        m_manager = BuildingManager.Instance;
        m_manager.Register(this);
    }

    public void TakeDamage(int damage)
    {
        m_health -= damage;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
