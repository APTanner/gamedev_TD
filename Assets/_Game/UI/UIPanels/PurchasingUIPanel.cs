public class PurchasingUIPanel : UIPanel
{
    private void OnEnable()
    {
        Switchboard.OnWaveStart += EventManager_OnWaveStart;
        Switchboard.OnWaveEnd += EventManager_OnWaveEnd;
    }
    private void OnDisable()
    {
        Switchboard.OnWaveStart -= EventManager_OnWaveStart;
        Switchboard.OnWaveEnd -= EventManager_OnWaveEnd;
    }

    private void EventManager_OnWaveStart(int wave)
    {
        DisablePanel();
    }

    private void EventManager_OnWaveEnd(int wave)
    {
        EnablePanel();
    }
}
