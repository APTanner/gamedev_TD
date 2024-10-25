using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwarmerSpawnAtPosition : MonoBehaviour
{
    public GameObject Prefab; // Assign the swarmer prefab in the Inspector
    public int swarmCount = 5; // Number of swarmers to spawn
    private bool isSpawningSwarmers = false; // Toggle boolean for enabling/disabling spawning

    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public List<GameObject> uiElementsToBlock;

    void Update()
    {
        // Check if the toggle is enabled and the left mouse button is clicked
        if (isSpawningSwarmers && Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverSpecificUI())
            {
                return;
            }
            Vector3 mousePosition = GetMouseWorldPosition();
            if (mousePosition != Vector3.zero)
            {
                Vector2 spawnPos = new Vector2(mousePosition.x, mousePosition.z);
                // Call the SpawnSwarmers method at the current mouse position
                SpawnSwarmers(spawnPos, swarmCount);
            }
        }
    }

    // Toggle the spawning boolean (this can be called by a button)
    public void ToggleSwarmersSpawning()
    {
        isSpawningSwarmers = !isSpawningSwarmers;
    }

    // Function to spawn swarmers in a pattern
    public void SpawnSwarmers(Vector2 pos, int count)
    {
        const float radius = 0.5f; // Hardcoding because this is temporary
        const float spacing = radius * 2;

        Instantiate(Prefab, new Vector3(pos.x, 0.5f, pos.y), Quaternion.identity);

        for (int i = 0, index = 1; index < count; ++i)
        {
            int steps = (i / 2) + 1;
            bool bVertical = i % 2 == 0;
            int sign = steps % 2 == 0 ? -1 : 1;

            Vector2 offset = new Vector2(
                bVertical ? 0 : sign,
                !bVertical ? 0 : sign);

            for (int j = 0; index < count && j < steps; ++j)
            {
                pos += offset * spacing;
                Instantiate(Prefab, new Vector3(pos.x, 0.5f, pos.y), Quaternion.identity);
                ++index;
            }
        }
    }

    // Helper function to get the mouse position in world space
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point; // Return the world position of the mouse click
        }

        return Vector3.zero; // Return zero if no valid position was found
    }

    private bool IsPointerOverSpecificUI()
    {
        PointerEventData eventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        uiRaycaster.Raycast(eventData, results);

        // Loop through all UI elements hit by the raycast and check if they are in the list of specific UI elements
        foreach (RaycastResult result in results)
        {
            if (uiElementsToBlock.Contains(result.gameObject))
            {
                return true; // Return true only if the hit UI element is one that should block the input
            }
        }

        return false; // No matching UI elements were hit
    }
}
