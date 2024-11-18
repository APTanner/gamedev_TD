using UnityEngine;
using System.Collections.Generic;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }

    public GameObject HQPrefab;
    public GameObject obstaclePrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject GetPrefabByTypeName(string typeName)
    {
        switch (typeName)
        {
            case "Obstacle":
                return obstaclePrefab;
            case "HQ":
                return HQPrefab;
            // Add other prefab cases here
            default:
                Debug.LogWarning($"No prefab found for type: {typeName}");
                return null;
        }
    }
}
