public class WinPanel : UIPanel
{
    private void OnEnable()
    {
        Switchboard.OnWin += EventManager_OnWin;
        Switchboard.OnLevelStart += EventManager_OnLevelStart;
    }
    private void OnDisable()
    {
        Switchboard.OnWin -= EventManager_OnWin;
        Switchboard.OnLevelStart -= EventManager_OnLevelStart;
    }

    private void EventManager_OnLevelStart(int obj)
    {
        DisablePanel();
    }

    private void EventManager_OnWin()
    {
        EnablePanel();
    }
}
