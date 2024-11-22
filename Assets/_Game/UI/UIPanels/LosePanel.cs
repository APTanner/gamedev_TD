public class LosePanel : UIPanel
{
    private void OnEnable()
    {
        Switchboard.OnLose += EventManager_OnLose;
        Switchboard.OnLevelStart += EventManager_OnLevelStart;
    }

    private void OnDisable()
    {
        Switchboard.OnLose -= EventManager_OnLose;
        Switchboard.OnLevelStart -= EventManager_OnLevelStart;
    }

    private void EventManager_OnLevelStart(int obj)
    {
        DisablePanel();
    }

    private void EventManager_OnLose()
    {
        EnablePanel();
    }
}
