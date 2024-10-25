using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmerSpawner : MonoBehaviour
{
    public SwarmerController Prefab;
    public int SpawnRate;

    private float m_counter = 0;

    protected void FixedUpdate()
    {
        float waitTime = 60 / (SpawnRate + Mathf.Epsilon);

        m_counter -= Time.fixedDeltaTime;
        if (m_counter < 0)
        {
            Instantiate(Prefab, transform.position, Quaternion.identity);
            m_counter += waitTime;
        }
    }
}
