using UnityEngine;

public class SellButton : MonoBehaviour
{
    public BuildingManager buildingManager;

    //public void OnSellButtonClick()
    //{
    //    // Assume the user selects a grid cell for selling
    //    Vector2Int selectedCoordinates = GetSelectedCoordinates();

    //    if (buildingManager != null)
    //    {
    //        buildingManager.SellBuilding(selectedCoordinates);
    //    }
    //}

    //private Vector2Int GetSelectedCoordinates()
    //{
    //    // Replace this with actual logic to get the coordinates of the building to sell
    //    Vector3 mousePosition = WorldMousePosition.Instance.Position;
    //    return GridManager.Instance.GetCoordinates(mousePosition);
    //}
}
