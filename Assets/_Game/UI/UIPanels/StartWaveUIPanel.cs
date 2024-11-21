public class StartWaveUIPanel : UIPanel
{
    private void OnEnable()
    {
        GameManager.OnWaveStart += GameManager_OnWaveStart;
        GameManager.OnWaveEnd += GameManager_OnWaveEnd;
    }

    private void OnDisable()
    {
        GameManager.OnWaveStart -= GameManager_OnWaveStart;
        GameManager.OnWaveEnd -= GameManager_OnWaveEnd;
    }

    private void GameManager_OnWaveStart(int wave)
    {
        DisablePanel();
    }

    private void GameManager_OnWaveEnd(int wave)
    {
        if (wave <= GameManager.Instance.WaveCount)
        {
            EnablePanel();
        }
    }
}
