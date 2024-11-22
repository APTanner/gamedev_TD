public class StartWaveUIPanel : UIPanel
{
    private void OnEnable()
    {
        Switchboard.OnLevelStart += Switchboard_OnLevelStart;
        Switchboard.OnWaveStart += EventManager_OnWaveStart;
        Switchboard.OnWaveEnd += EventManager_OnWaveEnd;
    }

    private void OnDisable()
    {
        Switchboard.OnLevelStart -= Switchboard_OnLevelStart;
        Switchboard.OnWaveStart -= EventManager_OnWaveStart;
        Switchboard.OnWaveEnd -= EventManager_OnWaveEnd;
    }

    private void EventManager_OnWaveStart(int wave)
    {
        DisablePanel();
    }

    private void Switchboard_OnLevelStart(int obj)
    {
        EnablePanel();
    }

    private void EventManager_OnWaveEnd(int wave)
    {
        if (GameManager.State != GameState.LevelOver)
        {
            EnablePanel();
        }
    }
}
