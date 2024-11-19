using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int currentWave = 0;

    public void BeginWave()
    {
        currentWave++;
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            // Check if the object has the EnemySpawner script attached
            EnemySpawner spawner = obj.GetComponent<EnemySpawner>();
            if (spawner != null)
            {
                spawner.SpawnSwarmers(currentWave);
            }
        }
    }
}
