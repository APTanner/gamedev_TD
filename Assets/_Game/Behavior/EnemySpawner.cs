using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyType;
    public int enemyNum;
    public int waveNum;

    public void SpawnSwarmers()
    {
        const float radius = 0.5f; // Hardcoding because this is temporary? This was copied from other spawn function so idk
        const float spacing = radius * 2;

        Vector3 pos = transform.position;
        Quaternion rotation = transform.rotation;

        Instantiate(enemyType, new Vector3(pos.x, 0.5f, pos.z), rotation);

        for (int i = 0, index = 1; index < enemyNum; ++i)
        {
            int steps = (i / 2) + 1;
            bool bVertical = i % 2 == 0;
            int sign = steps % 2 == 0 ? -1 : 1;

            Vector3 offset = new Vector3(
                bVertical ? 0 : sign,
                0,
                !bVertical ? 0 : sign);

            for (int j = 0; index < enemyNum && j < steps; ++j)
            {
                pos += offset * spacing;
                Instantiate(enemyType, new Vector3(pos.x, 0.5f, pos.z), rotation);
                ++index;
            }
        }
    }
}
