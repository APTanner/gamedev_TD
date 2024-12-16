using UnityEngine;

public class MenuGameManager : MonoBehaviour
{
    private bool setupDone = false;
    private Coroutine waveCoroutine;

    private void Update()
    {
        if (!setupDone)
        {
            TryInitialize();
        }
    }

    private void TryInitialize()
    {
        if (GameManager.Instance != null && GameManager.Instance.LevelData != null)
        {
            Switchboard.WaveStart(1);
            setupDone = true;

            // Start the coroutine to repeatedly start waves
            waveCoroutine = StartCoroutine(WaveLoop());
        }
    }

    private System.Collections.IEnumerator WaveLoop()
    {
        while (true)
        {
            // Wait a random amount of time between 10 and 30 seconds
            float waitTime = Random.Range(10f, 30f);
            yield return new WaitForSeconds(waitTime);

            // Call WaveStart(1) again
            Switchboard.WaveStart(1);
        }
    }
}
