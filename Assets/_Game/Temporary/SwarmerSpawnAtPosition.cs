using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmerSpawnAtPosition : MonoBehaviour
{
    public GameObject Prefab; // Assign the swarmer prefab in the Inspector
    public int swarmCount = 5; // Number of swarmers to spawn
    private bool isSpawningSwarmers = false; // Toggle boolean for enabling/disabling spawning

    void Update()
    {
        // Check if the toggle is enabled and the left mouse button is clicked
        if (isSpawningSwarmers && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector2 spawnPos = new Vector2(mousePosition.x, mousePosition.z);

            // Call the SpawnSwarmers method at the current mouse position
            SpawnSwarmers(spawnPos, swarmCount);
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
}
