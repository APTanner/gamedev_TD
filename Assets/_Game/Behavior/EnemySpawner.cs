using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyType;
    public int enemyNum;
    public int waveNum;

    private Renderer m_renderer;
    private MaterialPropertyBlock m_mpb;

    protected void Awake()
    {
        m_renderer = GetComponentInChildren<Renderer>();

        m_mpb = new MaterialPropertyBlock();

        m_renderer.enabled = false;
    }

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

    public void OnEnable()
    {
        Switchboard.OnLevelStart += Switchboard_OnLevelStart;
        Switchboard.OnWaveStart += Switchboard_OnWaveStart;
        Switchboard.OnWaveEnd += Switchboard_OnWaveEnd;
    }


    public void OnDisable()
    {
        Switchboard.OnLevelStart -= Switchboard_OnLevelStart;
        Switchboard.OnWaveStart -= Switchboard_OnWaveStart;
        Switchboard.OnWaveEnd -= Switchboard_OnWaveEnd;
    }
    private void Switchboard_OnLevelStart(int levelIndex)
    {
        if (waveNum == 1)
        {
            DissolveIn();
        }
    }

    private void Switchboard_OnWaveStart(int currentLevel)
    {
        if (currentLevel == waveNum)
        {
            SpawnSwarmers();
            DissolveOut();
        }
    }

    private void Switchboard_OnWaveEnd(int nextLevel)
    {
        if (nextLevel == waveNum)
        {
            DissolveIn();
        }
    }

    private void DissolveIn()
    {
        m_renderer.enabled = true;

        float size = 1 + enemyNum * .2f;
        transform.localScale = Vector3.one * size;
        m_coroutine = StartCoroutine(Dissolve(true, Defines.EnemySpawnMarkerFadeInTime));
    }

    private void DissolveOut()
    {
        m_coroutine = StartCoroutine(Dissolve(false, Defines.EnemySpwanMarkerFadeOutTime));
    }


    private Coroutine m_coroutine;
    private IEnumerator Dissolve(bool dissolveIn, float duration)
    {
        float elapsed = 0f;

        float startValue = dissolveIn ? 1 : 0;
        float endValue = dissolveIn ? 0 : 1;
        while (elapsed <= duration)
        {
            float value = Mathf.Lerp(startValue, endValue, elapsed / duration);

            UpdateDissolve(value);

            elapsed += Time.deltaTime;

            yield return null;
        }
        UpdateDissolve(endValue);
    }

    private void OnDestroy()
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
        }
    }

    private void UpdateDissolve(float value)
    {
        m_renderer.GetPropertyBlock(m_mpb);
        m_mpb.SetFloat("_Dissolve", value);
        m_renderer.SetPropertyBlock(m_mpb);
    }
}
